namespace Fixie.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;

    class Runner
    {
        readonly Assembly assembly;
        readonly Bus bus;
        readonly string[] customArguments;

        public Runner(Assembly assembly, Bus bus)
            : this(assembly, bus, new string[] {}) { }

        public Runner(Assembly assembly, Bus bus, string[] customArguments)
        {
            this.assembly = assembly;
            this.bus = bus;
            this.customArguments = customArguments;
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

            return Run(types, method => request[method.ReflectedType.FullName].Contains(method.Name));
        }

        public ExecutionSummary Run(Func<Type, bool> classCondition)
        {
            var candidateTypes = assembly.GetTypes().Where(classCondition).ToList();
            return Run(candidateTypes);
        }

        public ExecutionSummary Run(Type candidateType)
        {
            if (candidateType.Assembly != assembly)
                throw new Exception(
                    $"Candidate test class '{candidateType.FullName}' cannot be executed for assembly " +
                    $"'{assembly.GetName().Name}', because it is not defined in that assembly.");

            return Run(new[] { candidateType });
        }

        ExecutionSummary Run(IReadOnlyList<Type> candidateTypes, Func<MethodInfo, bool> methodCondition = null)
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

        internal ExecutionSummary Run(IReadOnlyList<Type> candidateTypes, Discovery discovery, Execution execution)
        {
            bus.Publish(new AssemblyStarted(assembly));

            var assemblySummary = new ExecutionSummary();
            var stopwatch = Stopwatch.StartNew();

            var classDiscoverer = new ClassDiscoverer(discovery);
            var classRunner = new ClassRunner(bus, discovery, execution);

            var testClasses = classDiscoverer.TestClasses(candidateTypes);

            bool isOnlyTestClass = testClasses.Count == 1;

            foreach (var testClass in testClasses)
            {
                var classSummary = classRunner.Run(testClass, isOnlyTestClass);
                assemblySummary.Add(classSummary);
            }

            stopwatch.Stop();
            bus.Publish(new AssemblyCompleted(assembly, assemblySummary, stopwatch.Elapsed));

            return assemblySummary;
        }
    }
}