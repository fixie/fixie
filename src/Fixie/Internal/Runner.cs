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
                    var test = new Test(testMethod);

                    if (testPattern.Matches(test))
                        matchingTests = matchingTests.Add(test.Name);
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
                await bus.PublishAsync(new TestDiscovered(new Test(testMethod)));
        }

        internal async Task<ExecutionSummary> RunAsync(IReadOnlyList<Type> candidateTypes, Discovery discovery, Execution execution, ImmutableHashSet<string> selectedTests)
        {
            var recorder = new ExecutionRecorder(bus);
            var classDiscoverer = new ClassDiscoverer(discovery);
            var classes = classDiscoverer.TestClasses(candidateTypes);
            var methodDiscoverer = new MethodDiscoverer(discovery);

            var testAssembly = new TestAssembly(assembly, selectedTests);
            await recorder.StartAsync(testAssembly);
            await execution.StartAsync();
            await RunAsync(testAssembly, selectedTests, recorder, classes, methodDiscoverer, execution);
            await execution.CompleteAsync();
            return await recorder.CompleteAsync(testAssembly);
        }

        static async Task RunAsync(
            TestAssembly testAssembly,
            ImmutableHashSet<string> selectedTests,
            ExecutionRecorder recorder,
            IReadOnlyList<Type> classes,
            MethodDiscoverer methodDiscoverer,
            Execution execution)
        {
            foreach (var @class in classes)
            {
                IEnumerable<MethodInfo> methods = methodDiscoverer.TestMethods(@class);

                if (!selectedTests.IsEmpty)
                    methods = methods.Where(method => selectedTests.Contains(new Test(method).Name));

                var classIsDisposable = IsDisposable(@class);
                var testMethods = methods
                    .Select(method => new TestMethod(recorder, classIsDisposable, method))
                    .ToList();

                if (testMethods.Any())
                {
                    var testClass = new TestClass(testAssembly, @class, testMethods);

                    Exception? classLifecycleFailure = null;

                    try
                    {
                        await execution.RunAsync(testClass);
                    }
                    catch (Exception exception)
                    {
                        classLifecycleFailure = exception;
                    }

                    foreach (var testMethod in testMethods)
                    {
                        var testNeverRan = !testMethod.RecordedResult;

                        if (classLifecycleFailure != null)
                            await testMethod.FailAsync(classLifecycleFailure);
                        
                        if (testNeverRan)
                            await testMethod.SkipAsync("This test did not run.");
                    }
                }
            }
        }

        static bool IsDisposable(Type @class)
            => @class.GetInterfaces().Any(x => x == typeof(IAsyncDisposable) || x == typeof(IDisposable));
    }
}