namespace Fixie.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Execution;

    public class Runner
    {
        readonly Bus bus;
        readonly Options options;

        public Runner(Bus bus)
            : this(bus, new Options()) { }

        public Runner(Bus bus, Options options)
        {
            this.bus = bus;
            this.options = options;
        }

        public void RunAssembly(Assembly assembly)
        {
            RunContext.Set(options);

            RunTypesInternal(assembly, assembly.GetTypes());
        }

        public void RunNamespace(Assembly assembly, string ns)
        {
            RunContext.Set(options);

            RunTypesInternal(assembly, assembly.GetTypes().Where(type => type.IsInNamespace(ns)).ToArray());
        }

        public void RunType(Assembly assembly, Type type)
        {
            RunContext.Set(options, type);

            var types = GetTypeAndNestedTypes(type).ToArray();
            RunTypesInternal(assembly, types);
        }

        public void RunTypes(Assembly assembly, Convention convention, params Type[] types)
        {
            RunContext.Set(options);

            Run(assembly, new[] { convention }, types);
        }

        public void RunMethods(Assembly assembly, params MethodInfo[] methods)
        {
            if (methods.Length == 1)
                RunContext.Set(options, methods.Single());
            else
                RunContext.Set(options);

            var conventions = GetConventions(assembly);

            foreach (var convention in conventions)
                convention.Methods.Where(methods.Contains);

            Run(assembly, conventions, methods.Select(m => m.ReflectedType).Distinct().ToArray());
        }

        public void RunMethods(Assembly assembly, MethodGroup[] methodGroups)
        {
            RunMethods(assembly, GetMethods(assembly, methodGroups));
        }

        static IEnumerable<Type> GetTypeAndNestedTypes(Type type)
        {
            yield return type;

            foreach (var nested in type.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic).SelectMany(GetTypeAndNestedTypes))
                yield return nested;
        }

        static MethodInfo[] GetMethods(Assembly assembly, MethodGroup[] methodGroups)
        {
            return methodGroups.SelectMany(methodGroup => GetMethods(assembly, methodGroup)).ToArray();
        }

        static IEnumerable<MethodInfo> GetMethods(Assembly assembly, MethodGroup methodGroup)
        {
            return assembly
                .GetType(methodGroup.Class)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.Name == methodGroup.Method);
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