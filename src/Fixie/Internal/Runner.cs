namespace Fixie.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Reports;

    class Runner
    {
        readonly Assembly assembly;
        readonly string[] customArguments;
        readonly Bus bus;

        public Runner(Assembly assembly, Report report)
            : this(assembly, new string[] {}, report) { }

        public Runner(Assembly assembly, string[] customArguments, params Report[] reports)
        {
            this.assembly = assembly;
            this.customArguments = customArguments;
            bus = new Bus(reports);
        }

        public async Task DiscoverAsync()
        {
            var discovery = new BehaviorDiscoverer(assembly, customArguments).GetDiscovery();

            try
            {
                await DiscoverAsync(assembly.GetTypes(), discovery);
            }
            finally
            {
                await discovery.DisposeIfApplicableAsync();
            }
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
            var discovery = new BehaviorDiscoverer(assembly, customArguments).GetDiscovery();

            try
            {
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
            finally
            {
                await discovery.DisposeIfApplicableAsync();
            }

            return await RunAsync(matchingTests);
        }

        async Task<ExecutionSummary> RunAsync(IReadOnlyList<Type> candidateTypes, ImmutableHashSet<string> selectedTests)
        {
            new BehaviorDiscoverer(assembly, customArguments)
                .GetBehaviors(out var discovery, out var execution);

            try
            {
                return await RunAsync(candidateTypes, discovery, execution, selectedTests);
            }
            finally
            {
                if (execution != discovery)
                    await execution.DisposeIfApplicableAsync();

                await discovery.DisposeIfApplicableAsync();
            }
        }

        internal async Task DiscoverAsync(IReadOnlyList<Type> candidateTypes, Discovery discovery)
        {
            var classDiscoverer = new ClassDiscoverer(discovery);
            var classes = classDiscoverer.TestClasses(candidateTypes);

            var methodDiscoverer = new MethodDiscoverer(discovery);
            foreach (var testClass in classes)
            foreach (var testMethod in methodDiscoverer.TestMethods(testClass))
                await bus.PublishAsync(new TestDiscovered(testMethod.TestName()));
        }

        internal async Task<ExecutionSummary> RunAsync(IReadOnlyList<Type> candidateTypes, Discovery discovery, Execution execution, ImmutableHashSet<string> selectedTests)
        {
            var recorder = new ExecutionRecorder(bus);
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
                    var classIsDisposable = IsDisposable(@class);
                    var testMethods = methods
                        .Select(method => new Test(recorder, classIsDisposable, method))
                        .ToList();

                    testClasses.Add(new TestClass(@class, testMethods));
                }
            }

            var testAssembly = new TestAssembly(assembly, testClasses);

            await recorder.StartAsync(testAssembly);
            await RunAsync(testAssembly, execution);
            return await recorder.CompleteAsync(testAssembly);
        }

        static async Task RunAsync(TestAssembly testAssembly, Execution execution)
        {
            Exception? assemblyLifecycleFailure = null;

            try
            {
                await execution.RunAsync(testAssembly);
            }
            catch (Exception exception)
            {
                assemblyLifecycleFailure = exception;
            }

            foreach (var test in testAssembly.Tests)
            {
                var testNeverRan = !test.RecordedResult;

                if (assemblyLifecycleFailure != null)
                    await test.FailAsync(assemblyLifecycleFailure);

                if (testNeverRan)
                    await test.SkipAsync("This test did not run.");
            }
        }

        static bool IsDisposable(Type @class)
            => @class.GetInterfaces().Any(x => x == typeof(IAsyncDisposable) || x == typeof(IDisposable));
    }
}