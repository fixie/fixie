namespace Fixie.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Reports;

    class Runner
    {
        readonly TestContext context;
        readonly IReport[] defaultReports;
        readonly Assembly assembly;
        readonly TextWriter console;

        public Runner(TestContext context, params IReport[] defaultReports)
        {
            this.context = context;
            this.defaultReports = defaultReports;
            assembly = context.Assembly;
            console = context.Console;
        }

        public async Task Discover()
        {
            var configuration = new ConfigurationDiscoverer(context).GetConfiguration();

            foreach (var convention in configuration.Conventions.Items)
                await Discover(assembly.GetTypes(), convention.Discovery);
        }

        public Task<ExecutionSummary> Run()
        {
            return Run(assembly.GetTypes(), ImmutableHashSet<string>.Empty);
        }

        public Task<ExecutionSummary> Run(ImmutableHashSet<string> selectedTests)
        {
            return Run(assembly.GetTypes(), selectedTests);
        }

        public async Task<ExecutionSummary> Run(TestPattern testPattern)
        {
            var matchingTests = ImmutableHashSet<string>.Empty;
            var configuration = new ConfigurationDiscoverer(context).GetConfiguration();

            foreach (var convention in configuration.Conventions.Items)
            {
                var discovery = convention.Discovery;

                var candidateTypes = assembly.GetTypes();
                var classDiscoverer = new ClassDiscoverer(discovery);
                var classes = classDiscoverer.TestClasses(candidateTypes);
                var methodDiscoverer = new MethodDiscoverer(discovery);
                foreach (var testClass in classes)
                    foreach (var testMethod in methodDiscoverer.TestMethods(testClass))
                    {
                        var test = testMethod.TestName();

                        if (testPattern.Matches(test))
                            matchingTests = matchingTests.Add(test);
                    }
            }

            return await Run(matchingTests);
        }

        async Task<ExecutionSummary> Run(IReadOnlyList<Type> candidateTypes, ImmutableHashSet<string> selectedTests)
        {
            var configuration = new ConfigurationDiscoverer(context).GetConfiguration();
            
            return await Run(candidateTypes, configuration, selectedTests);
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

        internal async Task<ExecutionSummary> Run(IReadOnlyList<Type> candidateTypes, Configuration configuration, ImmutableHashSet<string> selectedTests)
        {
            var conventions = configuration.Conventions.Items;
            var bus = new Bus(console, defaultReports.Concat(configuration.Reports.Items).ToArray());

            var recordingConsole = new RecordingWriter(console);
            var recorder = new ExecutionRecorder(recordingConsole, bus);
            
            using (new ConsoleRedirectionBoundary())
            {
                Console.SetOut(recordingConsole);
                await recorder.Start(assembly);

                foreach (var convention in conventions)
                {
                    var testSuite = BuildTestSuite(candidateTypes, convention.Discovery, selectedTests, recorder);
                    await Run(testSuite, convention.Execution);
                }

                return await recorder.Complete(assembly);
            }
        }

        static TestSuite BuildTestSuite(IReadOnlyList<Type> candidateTypes, IDiscovery discovery, ImmutableHashSet<string> selectedTests, ExecutionRecorder recorder)
        {
            var classDiscoverer = new ClassDiscoverer(discovery);
            var classes = classDiscoverer.TestClasses(candidateTypes);
            var methodDiscoverer = new MethodDiscoverer(discovery);

            var testClasses = new List<TestClass>(selectedTests.Count > 0 ? 0 : classes.Count);
            var selectionWorkingList = new List<MethodInfo>();

            foreach (var @class in classes)
            {
                var methods = methodDiscoverer.TestMethods(@class);

                if (!selectedTests.IsEmpty)
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
    }
}