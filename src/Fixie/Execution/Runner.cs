namespace Fixie.Execution
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using Listeners;

    class Runner
    {
        readonly Bus bus;
        readonly Filter filter;
        readonly string[] conventionArguments;

        public Runner(Bus bus)
            : this(bus, new Filter(), new string[] {}) { }

        public Runner(Bus bus, Filter filter, string[] conventionArguments)
        {
            this.bus = bus;
            this.filter = filter;
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
            return Run(assembly, new[] { convention }, types);
        }

        public ExecutionSummary RunMethods(Assembly assembly, PipeMessage.Test[] tests)
        {
            var types = GetTypes(assembly, tests);

            var methods = GetMethods(types, tests);

            var conventions = GetConventions(assembly);

            foreach (var convention in conventions)
                convention.Methods.Where(methods.Contains);

            return Run(assembly, conventions, types.Values.ToArray());
        }

        public ExecutionSummary RunMethod(Assembly assembly, MethodInfo method)
        {
            var conventions = GetConventions(assembly);

            foreach (var convention in conventions)
                convention.Methods.Where(m => m == method);

            return Run(assembly, conventions, method.ReflectedType);
        }

        static IEnumerable<Type> GetTypeAndNestedTypes(Type type)
        {
            yield return type;

            foreach (var nested in type.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic).SelectMany(GetTypeAndNestedTypes))
                yield return nested;
        }

        static Dictionary<string, Type> GetTypes(Assembly assembly, PipeMessage.Test[] tests)
        {
            var types = new Dictionary<string, Type>();

            foreach (var test in tests)
                if (!types.ContainsKey(test.Class))
                    types.Add(test.Class, assembly.GetType(test.Class));

            return types;
        }

        static MethodInfo[] GetMethods(Dictionary<string, Type> classes, PipeMessage.Test[] tests)
        {
            return tests
                .SelectMany(methodGroup =>
                    classes[methodGroup.Class]
                        .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                        .Where(m => m.Name == methodGroup.Method)).ToArray();
        }

        ExecutionSummary RunTypesInternal(Assembly assembly, params Type[] types)
        {
            return Run(assembly, GetConventions(assembly), types);
        }

        Convention[] GetConventions(Assembly assembly)
        {
            return new ConventionDiscoverer(assembly, conventionArguments).GetConventions();
        }

        ExecutionSummary Run(Assembly assembly, IEnumerable<Convention> conventions, params Type[] candidateTypes)
        {
            bus.Publish(new AssemblyStarted(assembly));

            var assemblySummary = new ExecutionSummary();
            var stopwatch = Stopwatch.StartNew();

            foreach (var convention in conventions)
                Run(convention, candidateTypes, assemblySummary);

            stopwatch.Stop();
            bus.Publish(new AssemblyCompleted(assembly, assemblySummary, stopwatch.Elapsed));

            return assemblySummary;
        }

        void Run(Convention convention, Type[] candidateTypes, ExecutionSummary assemblySummary)
        {
            var classDiscoverer = new ClassDiscoverer(convention);
            var classRunner = new ClassRunner(bus, filter, convention);

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