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
            return RunTypesInternal(assembly, assembly.GetTypes());
        }

        public ExecutionSummary RunNamespace(Assembly assembly, string ns)
        {
            return RunTypesInternal(assembly, assembly.GetTypes().Where(type => type.IsInNamespace(ns)).ToArray());
        }

        public ExecutionSummary RunType(Assembly assembly, Type type)
        {
            var types = GetTypeAndNestedTypes(type).ToArray();
            return RunTypesInternal(assembly, types);
        }

        public ExecutionSummary RunTypes(Assembly assembly, Discovery discovery, Lifecycle lifecycle, params Type[] types)
        {
            return Run(assembly, discovery, lifecycle, types);
        }

        public ExecutionSummary RunTests(Assembly assembly, Test[] tests)
        {
            var types = GetTypes(assembly, tests);

            var methods = GetMethods(types, tests);

            GetBehaviors(assembly, out var discovery, out var lifecycle);

            discovery.Methods.Where(methods.Contains);

            return Run(assembly, discovery, lifecycle, types.Values.ToArray());
        }

        public ExecutionSummary RunMethod(Assembly assembly, MethodInfo method)
        {
            GetBehaviors(assembly, out var discovery, out var lifecycle);

            discovery.Methods.Where(m => m == method);

            return Run(assembly, discovery, lifecycle, method.ReflectedType);
        }

        static IEnumerable<Type> GetTypeAndNestedTypes(Type type)
        {
            yield return type;

            foreach (var nested in type.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic).SelectMany(GetTypeAndNestedTypes))
                yield return nested;
        }

        static Dictionary<string, Type> GetTypes(Assembly assembly, Test[] tests)
        {
            var types = new Dictionary<string, Type>();

            foreach (var test in tests)
                if (!types.ContainsKey(test.Class))
                    types.Add(test.Class, assembly.GetType(test.Class));

            return types;
        }

        static MethodInfo[] GetMethods(Dictionary<string, Type> classes, Test[] tests)
        {
            return tests
                .SelectMany(test =>
                    classes[test.Class]
                        .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                        .Where(m => m.Name == test.Method)).ToArray();
        }

        ExecutionSummary RunTypesInternal(Assembly assembly, params Type[] types)
        {
            GetBehaviors(assembly, out var discovery, out var lifecycle);

            return Run(assembly, discovery, lifecycle, types);
        }

        void GetBehaviors(Assembly assembly, out Discovery discovery, out Lifecycle lifecycle)
        {
            new BehaviorDiscoverer(assembly, customArguments)
                .GetBehaviors(out discovery, out lifecycle);
        }

        ExecutionSummary Run(Assembly assembly, Discovery discovery, Lifecycle lifecycle, params Type[] candidateTypes)
        {
            bus.Publish(new AssemblyStarted(assembly));

            var assemblySummary = new ExecutionSummary();
            var stopwatch = Stopwatch.StartNew();

            Run(discovery, lifecycle, candidateTypes, assemblySummary);

            stopwatch.Stop();
            bus.Publish(new AssemblyCompleted(assembly, assemblySummary, stopwatch.Elapsed));

            return assemblySummary;
        }

        void Run(Discovery discovery, Lifecycle lifecycle, Type[] candidateTypes, ExecutionSummary assemblySummary)
        {
            var classDiscoverer = new ClassDiscoverer(discovery);
            var classRunner = new ClassRunner(bus, discovery, lifecycle);

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