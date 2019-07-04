namespace Fixie.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;

    class Runner
    {
        readonly Bus bus;
        readonly string[] customArguments;

        public Runner(Bus bus)
            : this(bus, new string[] {}) { }

        public Runner(Bus bus, string[] customArguments)
        {
            this.bus = bus;
            this.customArguments = customArguments;
        }

        public ExecutionSummary Run(Assembly assembly)
        {
            return Run(assembly, assembly.GetTypes());
        }

        public ExecutionSummary Run(Assembly assembly, Type[] candidateTypes, Func<MethodInfo, bool> methodCondition = null)
        {
            new BehaviorDiscoverer(assembly, customArguments)
                .GetBehaviors(out var discovery, out var execution);

            try
            {
                if (methodCondition != null)
                    discovery.Methods.Where(methodCondition);

                return Run(assembly, candidateTypes, discovery, execution);
            }
            finally
            {
                discovery.Dispose();

                if (execution != discovery)
                    execution.Dispose();
            }
        }

        public ExecutionSummary Run(Assembly assembly, Type[] candidateTypes, Discovery discovery, Execution execution)
        {
            bus.Publish(new AssemblyStarted(assembly));

            var assemblySummary = new ExecutionSummary();
            var stopwatch = Stopwatch.StartNew();

            Run(discovery, execution, candidateTypes, assemblySummary);

            stopwatch.Stop();
            bus.Publish(new AssemblyCompleted(assembly, assemblySummary, stopwatch.Elapsed));

            return assemblySummary;
        }

        public ExecutionSummary Run(Assembly assembly, Test[] tests)
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

            return Run(assembly, types.ToArray(), method => request[method.ReflectedType.FullName].Contains(method.Name));
        }

        void Run(Discovery discovery, Execution execution, Type[] candidateTypes, ExecutionSummary assemblySummary)
        {
            var classDiscoverer = new ClassDiscoverer(discovery);
            var classRunner = new ClassRunner(bus, discovery, execution);

            var testClasses = classDiscoverer.TestClasses(candidateTypes);

            bool isOnlyTestClass = testClasses.Count == 1;

            foreach (var testClass in testClasses)
            {
                var classSummary = classRunner.Run(testClass, isOnlyTestClass);
                assemblySummary.Add(classSummary);
            }
        }
    }
}