namespace Fixie.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
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

        public ExecutionSummary RunAssembly(Assembly assembly)
        {
            return Run(assembly, assembly.GetTypes());
        }

        public ExecutionSummary RunNamespace(Assembly assembly, string ns)
        {
            return Run(assembly, assembly.GetTypes().Where(type => type.IsInNamespace(ns)).ToArray());
        }

        public ExecutionSummary RunType(Assembly assembly, Type type)
        {
            return Run(assembly, GetTypeAndNestedTypes(type).ToArray());
        }

        public ExecutionSummary RunTypes(Assembly assembly, Discovery discovery, Execution execution, params Type[] types)
        {
            return Run(assembly, discovery, execution, types);
        }

        public ExecutionSummary RunTests(Assembly assembly, Test[] tests)
        {
            var request = new Dictionary<string, HashSet<string>>();

            foreach (var test in tests)
            {
                if (!request.ContainsKey(test.Class))
                    request.Add(test.Class, new HashSet<string>());

                request[test.Class].Add(test.Method);
            }

            var types = new List<Type>();
            var methods = new List<MethodInfo>();

            foreach (var testClass in request.Keys)
            {
                var type = assembly.GetType(testClass);

                if (type != null)
                {
                    types.Add(type);

                    var methodsToInclude = request[testClass];

                    methods.AddRange(type
                        .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                        .Where(m => methodsToInclude.Contains(m.Name))
                        .ToArray());
                }
            }

            return Run(assembly, types.ToArray(), methods.Contains);
        }

        public ExecutionSummary RunMethod(Assembly assembly, MethodInfo method)
        {
            return Run(assembly, new[] { method.ReflectedType }, m => m == method);
        }

        static IEnumerable<Type> GetTypeAndNestedTypes(Type type)
        {
            yield return type;

            foreach (var nested in type.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic).SelectMany(GetTypeAndNestedTypes))
                yield return nested;
        }

        ExecutionSummary Run(Assembly assembly, Type[] candidateTypes, Func<MethodInfo, bool> methodCondition = null)
        {
            new BehaviorDiscoverer(assembly, customArguments)
                .GetBehaviors(out var discovery, out var execution);

            try
            {
                if (methodCondition != null)
                    discovery.Methods.Where(methodCondition);

                return Run(assembly, discovery, execution, candidateTypes);
            }
            finally
            {
                discovery.Dispose();

                if (execution != discovery)
                    execution.Dispose();
            }
        }

        ExecutionSummary Run(Assembly assembly, Discovery discovery, Execution execution, Type[] candidateTypes)
        {
            bus.Publish(new AssemblyStarted(assembly));

            var assemblySummary = new ExecutionSummary();
            var stopwatch = Stopwatch.StartNew();

            Run(discovery, execution, candidateTypes, assemblySummary);

            stopwatch.Stop();
            bus.Publish(new AssemblyCompleted(assembly, assemblySummary, stopwatch.Elapsed));

            return assemblySummary;
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