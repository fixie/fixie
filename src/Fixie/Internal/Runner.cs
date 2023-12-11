using System.Reflection;
using Fixie.Reports;

namespace Fixie.Internal;

class Runner(TestEnvironment environment, params IReport[] reports)
{
    readonly Assembly assembly = environment.Assembly;
    readonly TextWriter console = environment.Console;

    public async Task Discover()
    {
        var configuration = BuildConfiguration();

        foreach (var convention in configuration.Conventions.Items)
            await Discover(assembly.GetTypes(), convention.Discovery);
    }

    public Task<ExecutionSummary> Run()
    {
        return Run(assembly.GetTypes(), new HashSet<string>());
    }

    public Task<ExecutionSummary> Run(HashSet<string> selectedTests)
    {
        return Run(assembly.GetTypes(), selectedTests);
    }

    public Task<ExecutionSummary> Run(TestPattern testPattern)
    {
        return Run(assembly.GetTypes(), new HashSet<string>(), testPattern);
    }

    async Task<ExecutionSummary> Run(IReadOnlyList<Type> candidateTypes, HashSet<string> selectedTests, TestPattern? testPattern = null)
    {
        var configuration = BuildConfiguration();
            
        return await Run(candidateTypes, configuration, selectedTests, testPattern);
    }

    internal async Task Discover(IReadOnlyList<Type> candidateTypes, IDiscovery discovery)
    {
        var bus = new Bus(console, reports);

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
        var bus = new Bus(console, reports.Concat(configuration.Reports.Items).ToArray());

        var recordingConsole = new RecordingWriter(console);
        var recorder = new ExecutionRecorder(recordingConsole, bus);
            
        using (new ConsoleRedirectionBoundary())
        {
            Console.SetOut(recordingConsole);
            await recorder.StartExecution();

            foreach (var convention in conventions)
            {
                var testSuite = BuildTestSuite(candidateTypes, convention.Discovery, selectedTests, testPattern, recorder);
                await Run(testSuite, convention.Execution);
            }

            return await recorder.CompleteExecution();
        }
    }

    static TestSuite BuildTestSuite(IReadOnlyList<Type> candidateTypes, IDiscovery discovery, HashSet<string> selectedTests, TestPattern? testPattern, ExecutionRecorder recorder)
    {
        var classDiscoverer = new ClassDiscoverer(discovery);
        var classes = classDiscoverer.TestClasses(candidateTypes);
        var methodDiscoverer = new MethodDiscoverer(discovery);

        var testClasses = new List<TestClass>(selectedTests.Count > 0 ? 0 : classes.Count);
        var selectionWorkingList = new List<MethodInfo>();

        foreach (var @class in classes)
        {
            var methods = methodDiscoverer.TestMethods(@class);

            if (selectedTests.Count > 0)
            {
                selectionWorkingList.AddRange(methods.Where(method => selectedTests.Contains(method.TestName())));

                if (selectionWorkingList.Count == 0)
                {
                    methods = Array.Empty<MethodInfo>();
                }
                else
                {
                    methods = selectionWorkingList;
                    selectionWorkingList = new List<MethodInfo>();
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