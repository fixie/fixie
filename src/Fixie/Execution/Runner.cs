namespace Fixie.Execution
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;

    class Runner
    {
        readonly Bus bus;
        readonly string[] conventionArguments;

        public Runner(Bus bus)
            : this(bus, new string[] {}) { }

        public Runner(Bus bus, string[] conventionArguments)
        {
            this.bus = bus;
            this.conventionArguments = conventionArguments;
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

        public ExecutionSummary RunTypes(Assembly assembly, Convention convention, params Type[] types)
        {
            return Run(assembly, convention, types);
        }

        public ExecutionSummary RunTests(Assembly assembly, Test[] tests)
        {
            var types = GetTypes(assembly, tests);

            var methods = GetMethods(types, tests);

            var convention = GetConvention(assembly);

            convention.Methods.Where(methods.Contains);

            return Run(assembly, convention, types.Values.ToArray());
        }

        public ExecutionSummary RunMethod(Assembly assembly, MethodInfo method)
        {
            var convention = GetConvention(assembly);

            convention.Methods.Where(m => m == method);

            return Run(assembly, convention, method.ReflectedType);
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
            return Run(assembly, GetConvention(assembly), types);
        }

        Convention GetConvention(Assembly assembly)
        {
            return new ConventionDiscoverer(assembly, conventionArguments).GetConvention();
        }

        ExecutionSummary Run(Assembly assembly, Convention convention, params Type[] candidateTypes)
        {
            bus.Publish(new AssemblyStarted(assembly));

            var assemblySummary = new ExecutionSummary();
            var stopwatch = Stopwatch.StartNew();

            Run(convention, candidateTypes, assemblySummary);

            stopwatch.Stop();
            bus.Publish(new AssemblyCompleted(assembly, assemblySummary, stopwatch.Elapsed));

            return assemblySummary;
        }

        void Run(Convention convention, Type[] candidateTypes, ExecutionSummary assemblySummary)
        {
            var classDiscoverer = new ClassDiscoverer(convention);
            var classRunner = new ClassRunner(bus, convention);

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