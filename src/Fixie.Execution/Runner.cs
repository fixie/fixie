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
        readonly Options options;

        public Runner(Bus bus)
            : this(bus, new Options()) { }

        public Runner(Bus bus, Options options)
        {
            this.bus = bus;
            this.options = options;
        }

        public AssemblyReport RunAssembly(Assembly assembly)
        {
            RunContext.Set(options);

            return RunTypesInternal(assembly, assembly.GetTypes());
        }

        public AssemblyReport RunNamespace(Assembly assembly, string ns)
        {
            RunContext.Set(options);

            return RunTypesInternal(assembly, assembly.GetTypes().Where(type => type.IsInNamespace(ns)).ToArray());
        }

        public AssemblyReport RunType(Assembly assembly, Type type)
        {
            RunContext.Set(options, type);

            var types = GetTypeAndNestedTypes(type).ToArray();
            return RunTypesInternal(assembly, types);
        }

        public AssemblyReport RunTypes(Assembly assembly, Convention convention, params Type[] types)
        {
            RunContext.Set(options);

            return Run(assembly, new[] { convention }, types);
        }

        public AssemblyReport RunMethods(Assembly assembly, params MethodInfo[] methods)
        {
            if (methods.Length == 1)
                RunContext.Set(options, methods.Single());
            else
                RunContext.Set(options);

            var conventions = GetConventions(assembly);

            foreach (var convention in conventions)
                convention.Methods.Where(methods.Contains);

            return Run(assembly, conventions, methods.Select(m => m.ReflectedType).Distinct().ToArray());
        }

        public AssemblyReport RunMethods(Assembly assembly, MethodGroup[] methodGroups)
        {
            return RunMethods(assembly, GetMethods(assembly, methodGroups));
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

        AssemblyReport RunTypesInternal(Assembly assembly, params Type[] types)
        {
            return Run(assembly, GetConventions(assembly), types);
        }

        static Convention[] GetConventions(Assembly assembly)
        {
            return new ConventionDiscoverer(assembly).GetConventions();
        }

        AssemblyReport Run(Assembly assembly, IEnumerable<Convention> conventions, params Type[] candidateTypes)
        {
            var assemblyReport = new AssemblyReport(assembly.Location);

            bus.Publish(new AssemblyStarted(assembly));

            foreach (var convention in conventions)
                Run(assemblyReport, convention, candidateTypes);

            bus.Publish(new AssemblyCompleted(assembly));

            return assemblyReport;
        }

        void Run(AssemblyReport assemblyReport, Convention convention, Type[] candidateTypes)
        {
            var classDiscoverer = new ClassDiscoverer(convention);
            var classRunner = new ClassRunner(bus, convention);

            foreach (var testClass in classDiscoverer.TestClasses(candidateTypes))
            {
                var classReport = classRunner.Run(testClass);

                assemblyReport.Add(classReport);
            }
        }
    }
}