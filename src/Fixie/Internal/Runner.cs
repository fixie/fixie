using System.Reflection;
using System.Threading.Channels;
using Fixie.Reports;

namespace Fixie.Internal;

class Runner
{
    readonly TestEnvironment environment;
    readonly IReport[] defaultReports;
    readonly Assembly assembly;
    readonly TextWriter console;

    public Runner(TestEnvironment environment, params IReport[] defaultReports)
    {
        this.environment = environment;
        this.defaultReports = defaultReports;
        assembly = environment.Assembly;
        console = environment.Console;
    }

    public async Task Discover()
    {
        var configuration = BuildConfiguration();

        foreach (var convention in configuration.Conventions.Items)
            await Discover(assembly.GetTypes(), convention.Discovery);
    }

    public Task<ExecutionSummary> Run()
    {
        return Run(assembly.GetTypes(), []);
    }

    public Task<ExecutionSummary> Run(HashSet<string> selectedTests)
    {
        return Run(assembly.GetTypes(), selectedTests);
    }

    public Task<ExecutionSummary> Run(TestPattern testPattern)
    {
        return Run(assembly.GetTypes(), [], testPattern);
    }

    async Task<ExecutionSummary> Run(IReadOnlyList<Type> candidateTypes, HashSet<string> selectedTests, TestPattern? testPattern = null)
    {
        var configuration = BuildConfiguration();
            
        return await Run(candidateTypes, configuration, selectedTests, testPattern);
    }

    internal async Task Discover(IReadOnlyList<Type> candidateTypes, IDiscovery discovery)
    {
        var bus = new Bus(console, defaultReports);

        var classDiscoverer = new ClassDiscoverer(discovery);
        var classes = classDiscoverer.TestClasses(candidateTypes);

        var methodDiscoverer = new MethodDiscoverer(discovery);
        foreach (var testClass in classes)
            foreach (var testMethod in methodDiscoverer.TestMethods(testClass))
                await bus.Publish(new TestDiscovered(testMethod.TestName()));
    }

    internal async Task<ExecutionSummary> Run(IReadOnlyList<Type> candidateTypes, TestConfiguration configuration, HashSet<string> selectedTests, TestPattern? testPattern = null)
    {
        var conventions = configuration.Conventions.Items;
        var bus = new Bus(console, defaultReports.Concat(configuration.Reports.Items).ToArray());

        using (new ConsoleRedirectionBoundary())
        {
            var channel = Channel.CreateUnbounded<IMessage>();

            var recorder = new ExecutionRecorder(channel);

            var consumer = Task.Run(async () =>
            {
                await foreach (var message in channel.Reader.ReadAllAsync())
                {
                    switch (message)
                    {
                        case ExecutionStarted x: await bus.Publish(x); break;
                        case TestStarted x: await bus.Publish(x); break;
                        case TestSkipped x: await bus.Publish(x); break;
                        case TestPassed x: await bus.Publish(x); break;
                        case TestFailed x: await bus.Publish(x); break;
                        case ExecutionCompleted x: await bus.Publish(x); break;
                    }
                }
            });

            await recorder.StartExecution();

            foreach (var convention in conventions)
            {
                var testSuite = BuildTestSuite(candidateTypes, convention.Discovery, selectedTests, testPattern, recorder);
                await Run(testSuite, convention.Execution);
            }

            var assemblySummary = await recorder.CompleteExecution();

            channel.Writer.Complete();

            await consumer;

            return assemblySummary;
        }
    }

    static TestSuite BuildTestSuite(IReadOnlyList<Type> candidateTypes, IDiscovery discovery, HashSet<string> selectedTests, TestPattern? testPattern, ExecutionRecorder recorder)
    {
        var classDiscoverer = new ClassDiscoverer(discovery);
        var classes = classDiscoverer.TestClasses(candidateTypes);
        var methodDiscoverer = new MethodDiscoverer(discovery);

        var testClasses = new List<TestClass>(selectedTests.Count > 0 ? 0 : classes.Count);
        List<MethodInfo> selectionWorkingList = [];

        foreach (var @class in classes)
        {
            var methods = methodDiscoverer.TestMethods(@class);

            if (selectedTests.Count > 0)
            {
                selectionWorkingList.AddRange(methods.Where(method => selectedTests.Contains(method.TestName())));

                if (selectionWorkingList.Count == 0)
                {
                    methods = [];
                }
                else
                {
                    methods = selectionWorkingList;
                    selectionWorkingList = [];
                }
            }

            if (testPattern != null)
                methods = methods.Where(method => testPattern.Matches(method.TestName())).ToList();

            if (methods.Count > 0)
            {
                var testMethods = methods
                    .Select(method => new Test(recorder, method))
                    .ToList();

                testClasses.Add(new TestClass(@class, testMethods));
            }
        }

        return new TestSuite(testClasses);
    }

    static async Task Run(TestSuite testSuite, IExecution execution)
    {
        Exception? assemblyLifecycleFailure = null;

        try
        {
            await execution.Run(testSuite);
        }
        catch (Exception exception)
        {
            assemblyLifecycleFailure = exception;
        }

        foreach (var test in testSuite.Tests)
        {
            var testNeverRan = !test.RecordedResult;

            if (assemblyLifecycleFailure != null)
                await test.Fail(assemblyLifecycleFailure);

            if (testNeverRan)
                await test.Skip("This test did not run.");
        }
    }

    TestConfiguration BuildConfiguration()
    {
        var customTestProjectTypes = assembly
            .GetTypes()
            .Where(type => IsTestProject(type) && !type.IsAbstract)
            .ToArray();

        if (customTestProjectTypes.Length > 1)
        {
            throw new Exception(
                "A test assembly can have at most one ITestProject implementation, " +
                "but the following implementations were discovered:" + Environment.NewLine +
                string.Join(Environment.NewLine,
                    customTestProjectTypes
                        .Select(x => $"\t{x.FullName}")));
        }

        var configuration = new TestConfiguration();

        var testProjectType = customTestProjectTypes.SingleOrDefault();
            
        if (testProjectType != null)
        {
            var testProject = (ITestProject) Construct(testProjectType);

            testProject.Configure(configuration, environment);
        }

        if (configuration.Conventions.Items.Count == 0)
            configuration.Conventions.Add<DefaultDiscovery, DefaultExecution>();

        return configuration;
    }

    static bool IsTestProject(Type type)
        => type.GetInterfaces().Contains(typeof(ITestProject));

    static object Construct(Type type)
    {
        try
        {
            return type.GetConstructors().Single().Invoke(null);
        }
        catch (Exception ex)
        {
            throw new Exception($"Could not construct an instance of type '{type.FullName}'.", ex);
        }
    }
}