namespace Fixie.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Reflection;
    using System.Threading.Tasks;

    class Runner
    {
        readonly Assembly assembly;
        readonly string[] customArguments;
        readonly Bus bus;

        public Runner(Assembly assembly, Listener listener)
            : this(assembly, new string[] {}, listener) { }

        public Runner(Assembly assembly, string[] customArguments, params Listener[] listeners)
        {
            this.assembly = assembly;
            this.customArguments = customArguments;
            bus = new Bus(listeners);
        }

        public void Discover()
        {
            var discovery = new BehaviorDiscoverer(assembly, customArguments).GetDiscovery();

            try
            {
                Discover(assembly.GetTypes(), discovery);
            }
            finally
            {
                discovery.Dispose();
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

        public Task<ExecutionSummary> RunAsync(TestPattern testPattern)
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
                discovery.Dispose();
            }

            return RunAsync(matchingTests);
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
                    execution.Dispose();

                discovery.Dispose();
            }
        }

        internal void Discover(IReadOnlyList<Type> candidateTypes, Discovery discovery)
        {
            var classDiscoverer = new ClassDiscoverer(discovery);
            var classes = classDiscoverer.TestClasses(candidateTypes);

            var methodDiscoverer = new MethodDiscoverer(discovery);
            foreach (var testClass in classes)
            foreach (var testMethod in methodDiscoverer.TestMethods(testClass))
                bus.PublishAsync(new TestDiscovered(new Test(testMethod)))
                    .GetAwaiter().GetResult();
        }

        internal async Task<ExecutionSummary> RunAsync(IReadOnlyList<Type> candidateTypes, Discovery discovery, Execution execution, ImmutableHashSet<string> selectedTests)
        {
            var recorder = new ExecutionRecorder(bus);
            var classDiscoverer = new ClassDiscoverer(discovery);
            var classes = classDiscoverer.TestClasses(candidateTypes);
            var methodDiscoverer = new MethodDiscoverer(discovery);

            var testAssembly = new TestAssembly(assembly, selectedTests, recorder, classes, methodDiscoverer, execution);
            recorder.Start(testAssembly);
            await testAssembly.RunAsync();
            return recorder.Complete(testAssembly);
        }
    }
}