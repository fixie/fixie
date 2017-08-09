namespace Fixie.Execution
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class Runner
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
            RunContext.Initialize();

            return RunTypesInternal(assembly, assembly.GetTypes());
        }

        public ExecutionSummary RunNamespace(Assembly assembly, string ns)
        {
            RunContext.Initialize();

            return RunTypesInternal(assembly, assembly.GetTypes().Where(type => type.IsInNamespace(ns)).ToArray());
        }

        public ExecutionSummary RunType(Assembly assembly, Type type)
        {
            RunContext.Initialize(type);

            var types = GetTypeAndNestedTypes(type).ToArray();
            return RunTypesInternal(assembly, types);
        }

        public ExecutionSummary RunTypes(Assembly assembly, Convention convention, params Type[] types)
        {
            RunContext.Initialize();

            return Run(assembly, new[] { convention }, types);
        }

        public ExecutionSummary RunMethods(Assembly assembly, MethodGroup[] methodGroups)
        {
            var types = GetTypes(assembly, methodGroups);

            var methods = GetMethods(types, methodGroups);

            if (methods.Length == 1)
                RunContext.Initialize(methods.Single());
            else
                RunContext.Initialize();

            var conventions = GetConventions(assembly);

            foreach (var convention in conventions)
                convention.Methods.Where(methods.Contains);

            return Run(assembly, conventions, types.Values.ToArray());
        }

        public ExecutionSummary RunMethods(Assembly assembly, Type type, MethodInfo method)
        {
            RunContext.Initialize(method);

            var conventions = GetConventions(assembly);

            foreach (var convention in conventions)
                convention.Methods.Where(m => m == method);

            return Run(assembly, conventions, type);
        }

        static IEnumerable<Type> GetTypeAndNestedTypes(Type type)
        {
            yield return type;

            foreach (var nested in type.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic).SelectMany(GetTypeAndNestedTypes))
                yield return nested;
        }

        static Dictionary<string, Type> GetTypes(Assembly assembly, MethodGroup[] methodGroups)
        {
            var types = new Dictionary<string, Type>();

            foreach (var methodGroup in methodGroups)
                if (!types.ContainsKey(methodGroup.Class))
                    types.Add(methodGroup.Class, assembly.GetType(methodGroup.Class));

            return types;
        }

        static MethodInfo[] GetMethods(Dictionary<string, Type> classes, MethodGroup[] methodGroups)
        {
            return methodGroups
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
            var assemblySummary = new ExecutionSummary();
            bus.Publish(new AssemblyStarted(assembly));

            foreach (var convention in conventions)
                Run(convention, candidateTypes, assemblySummary);

            bus.Publish(new AssemblyCompleted(assembly, assemblySummary));

            return assemblySummary;
        }

        void Run(Convention convention, Type[] candidateTypes, ExecutionSummary assemblySummary)
        {
            var classDiscoverer = new ClassDiscoverer(convention);
            var classRunner = new ClassRunner(bus, convention);

            foreach (var testClass in classDiscoverer.TestClasses(candidateTypes))
            {
                var classSummary = classRunner.Run(testClass);
                assemblySummary.Add(classSummary);
            }
        }
    }
}