namespace Fixie.Execution
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Internal;

    public class Runner
    {
        readonly Bus bus;
        readonly string[] conventionArguments;

        public Runner(Bus bus, params string[] conventionArguments)
        {
            this.bus = bus;
            this.conventionArguments = conventionArguments;
        }

        public void RunAssembly(Assembly assembly)
        {
            RunContext.Set(conventionArguments);

            RunTypesInternal(assembly, assembly.GetTypes());
        }

        public void RunNamespace(Assembly assembly, string ns)
        {
            RunContext.Set(conventionArguments);

            RunTypesInternal(assembly, assembly.GetTypes().Where(type => type.IsInNamespace(ns)).ToArray());
        }

        public void RunType(Assembly assembly, Type type)
        {
            RunContext.Set(conventionArguments, type);

            var types = GetTypeAndNestedTypes(type).ToArray();
            RunTypesInternal(assembly, types);
        }

        public void RunTypes(Assembly assembly, Convention convention, params Type[] types)
        {
            RunContext.Set(conventionArguments);

            Run(assembly, new[] { convention }, types);
        }

        public void RunMethod(Assembly assembly, Method method)
        {
            RunContext.Set(conventionArguments, method.MethodInfo);

            var conventions = GetConventions(assembly);

            foreach (var convention in conventions)
                convention.Methods.Where(m => m == method.MethodInfo);

            Run(assembly, conventions, method.Class);
        }

        public void RunMethods(Assembly assembly, Method[] methods)
        {
            var methodInfos = methods.Select(m => m.MethodInfo).ToArray();

            if (methodInfos.Length == 1)
                RunContext.Set(conventionArguments, methodInfos.Single());
            else
                RunContext.Set(conventionArguments);

            var conventions = GetConventions(assembly);

            foreach (var convention in conventions)
                convention.Methods.Where(methodInfos.Contains);

            Run(assembly, conventions, methods.Select(m => m.Class).Distinct().ToArray());
        }

        static IEnumerable<Type> GetTypeAndNestedTypes(Type type)
        {
            yield return type;

            foreach (var nested in type.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic).SelectMany(GetTypeAndNestedTypes))
                yield return nested;
        }

        void RunTypesInternal(Assembly assembly, params Type[] types)
        {
            Run(assembly, GetConventions(assembly), types);
        }

        static Convention[] GetConventions(Assembly assembly)
        {
            return new ConventionDiscoverer(assembly).GetConventions();
        }

        void Run(Assembly assembly, IEnumerable<Convention> conventions, params Type[] candidateTypes)
        {
            bus.Publish(new AssemblyStarted(assembly));

            foreach (var convention in conventions)
                Run(convention, candidateTypes);

            bus.Publish(new AssemblyCompleted(assembly));
        }

        void Run(Convention convention, Type[] candidateTypes)
        {
            var classDiscoverer = new ClassDiscoverer(convention);
            var classRunner = new ClassRunner(bus, convention);

            foreach (var testClass in classDiscoverer.TestClasses(candidateTypes))
                classRunner.Run(testClass);
        }
    }
}