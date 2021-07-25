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

        public Runner(TestContext context, params IReport[] reports)
        {
            this.context = context;
            assembly = context.Assembly;
            console = context.Console;
            bus = new Bus(console, reports);
        }

        public async Task DiscoverAsync()
        {
            var conventions = new ConventionDiscoverer(context).GetConventions();

            foreach (var convention in conventions)
                await DiscoverAsync(assembly.GetTypes(), convention.Discovery);
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
            var conventions = new ConventionDiscoverer(context).GetConventions();

            foreach (var convention in conventions)
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

            return await RunAsync(matchingTests);
        }

        async Task<ExecutionSummary> RunAsync(IReadOnlyList<Type> candidateTypes, ImmutableHashSet<string> selectedTests)
        {
            var conventions = new ConventionDiscoverer(context).GetConventions();

            return await RunAsync(candidateTypes, conventions, selectedTests);
        }

        internal async Task DiscoverAsync(IReadOnlyList<Type> candidateTypes, IDiscovery discovery)
        {
            var classDiscoverer = new ClassDiscoverer(discovery);
            var classes = classDiscoverer.TestClasses(candidateTypes);

            var methodDiscoverer = new MethodDiscoverer(discovery);
            foreach (var testClass in classes)
            foreach (var testMethod in methodDiscoverer.TestMethods(testClass))
                await bus.Publish(new TestDiscovered(testMethod.TestName()));
        }

        internal async Task<ExecutionSummary> RunAsync(IReadOnlyList<Type> candidateTypes, IReadOnlyList<Convention> conventions, ImmutableHashSet<string> selectedTests)
        {
            var recordingConsole = new RecordingWriter(console);
            var recorder = new ExecutionRecorder(recordingConsole, bus);
            
            using (new ConsoleRedirectionBoundary())
            {
                Console.SetOut(recordingConsole);
                await recorder.Start(assembly);

                foreach (var convention in conventions)
                {
                    var testSuite = BuildTestSuite(candidateTypes, convention.Discovery, selectedTests, recorder);
                    await RunAsync(testSuite, convention.Execution);
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

        static async Task RunAsync(TestSuite testSuite, IExecution execution)
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