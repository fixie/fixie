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
        readonly Assembly assembly;
        readonly Bus bus;
        readonly TextWriter console;

        public Runner(TestContext context, params Report[] reports)
        {
            this.context = context;
            assembly = context.Assembly;
            console = context.Console;
            bus = new Bus(console, reports);
        }

        public async Task DiscoverAsync()
        {
            var discovery = new ConventionDiscoverer(context).GetConvention().Discovery;

            await DiscoverAsync(assembly.GetTypes(), discovery);
        }

        public Task<ExecutionSummary> RunAsync()
        {
            return RunAsync(assembly.GetTypes(), ImmutableHashSet<string>.Empty);
        }

        public Task<ExecutionSummary> RunAsync(ImmutableHashSet<string> selectedTests)
        {
            return RunAsync(assembly.GetTypes(), selectedTests);
        }

        public async Task<ExecutionSummary> RunAsync(TestPattern testPattern)
        {
            var matchingTests = ImmutableHashSet<string>.Empty;
            var discovery = new ConventionDiscoverer(context).GetConvention().Discovery;

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

            return await RunAsync(matchingTests);
        }

        async Task<ExecutionSummary> RunAsync(IReadOnlyList<Type> candidateTypes, ImmutableHashSet<string> selectedTests)
        {
            var convention = new ConventionDiscoverer(context).GetConvention();

            var discovery = convention.Discovery;
            var execution = convention.Execution;

            return await RunAsync(candidateTypes, discovery, execution, selectedTests);
        }

        internal async Task DiscoverAsync(IReadOnlyList<Type> candidateTypes, IDiscovery discovery)
        {
            var classDiscoverer = new ClassDiscoverer(discovery);
            var classes = classDiscoverer.TestClasses(candidateTypes);

            var methodDiscoverer = new MethodDiscoverer(discovery);
            foreach (var testClass in classes)
            foreach (var testMethod in methodDiscoverer.TestMethods(testClass))
                await bus.PublishAsync(new TestDiscovered(testMethod.TestName()));
        }

        internal async Task<ExecutionSummary> RunAsync(IReadOnlyList<Type> candidateTypes, IDiscovery discovery, IExecution execution, ImmutableHashSet<string> selectedTests)
        {
            var recordingConsole = new RecordingWriter(console);
            var recorder = new ExecutionRecorder(recordingConsole, bus);
            
            using (new ConsoleRedirectionBoundary())
            {
                Console.SetOut(recordingConsole);
                await recorder.StartAsync(assembly);

                var testSuite = BuildTestSuite(candidateTypes, discovery, selectedTests, recorder);
                await RunAsync(testSuite, execution);

                return await recorder.CompleteAsync(assembly);
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

        static async Task RunAsync(TestSuite testSuite, IExecution execution)
        {
            Exception? assemblyLifecycleFailure = null;

            try
            {
                await execution.RunAsync(testSuite);
            }
            catch (Exception exception)
            {
                assemblyLifecycleFailure = exception;
            }

            foreach (var test in testSuite.Tests)
            {
                var testNeverRan = !test.RecordedResult;

                if (assemblyLifecycleFailure != null)
                    await test.FailAsync(assemblyLifecycleFailure);

                if (testNeverRan)
                    await test.SkipAsync("This test did not run.");
            }
        }
    }
}