namespace Fixie.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    class TestAssembly
    {
        readonly Assembly assembly;
        readonly string[] customArguments;
        readonly Bus bus;

        public TestAssembly(Assembly assembly, Listener listener)
            : this(assembly, new string[] {}, listener) { }

        public TestAssembly(Assembly assembly, string[] customArguments, params Listener[] listeners)
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

        public ExecutionSummary Run()
        {
            return Run(assembly.GetTypes());
        }

        public ExecutionSummary Run(IReadOnlyList<Test> tests)
        {
            var request = new Dictionary<string, HashSet<string>>();
            var types = new List<Type>();

            foreach (var test in tests)
            {
                if (!request.ContainsKey(test.Class))
                {
                    request.Add(test.Class, new HashSet<string>());

                    var type = assembly.GetType(test.Class);

                    if (type != null)
                        types.Add(type);
                }

                request[test.Class].Add(test.Method);
            }

            return Run(types, method => request[method.ReflectedType!.FullName!].Contains(method.Name));
        }

        ExecutionSummary Run(IReadOnlyList<Type> candidateTypes, Func<MethodInfo, bool>? methodCondition = null)
        {
            new BehaviorDiscoverer(assembly, customArguments)
                .GetBehaviors(out var discovery, out var execution);

            try
            {
                if (methodCondition != null)
                    discovery.Methods.Where(methodCondition);

                return Run(candidateTypes, discovery, execution);
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
            var testClasses = classDiscoverer.TestClasses(candidateTypes);

            var methodDiscoverer = new MethodDiscoverer(discovery);
            foreach (var testClass in testClasses)
            foreach (var testMethod in methodDiscoverer.TestMethods(testClass))
                bus.Publish(new TestDiscovered(new Test(testMethod)));
        }

        internal ExecutionSummary Run(IReadOnlyList<Type> candidateTypes, Discovery discovery, Execution execution)
        {
            var recorder = new ExecutionRecorder(bus);
            
            recorder.Start(assembly);
            
            var classDiscoverer = new ClassDiscoverer(discovery);
            var methodDiscoverer = new MethodDiscoverer(discovery);
            var parameterGenerator = new ParameterGenerator(discovery);
            var classRunner = new ClassRunner(recorder, parameterGenerator, execution);

            var testClasses = classDiscoverer.TestClasses(candidateTypes);

            foreach (var testClass in testClasses)
            {
                var testMethods = methodDiscoverer.TestMethods(testClass);

                if (testMethods.Any())
                {
                    var targetMethod = testClasses.Count == 1 && testMethods.Count == 1
                        ? testMethods.Single()
                        : null;

                    classRunner.Run(testClass, testMethods, targetMethod);
                }
            }

            return recorder.Complete(assembly);
        }
    }
}