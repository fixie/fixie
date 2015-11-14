using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fixie.Execution;

namespace Fixie.Internal
{
    public class Runner
    {
        readonly Listener listener;
        readonly Options options;

        public Runner(Listener listener)
            : this(listener, new Options()) { }

        public Runner(Listener listener, Options options)
        {
            this.listener = listener;
            this.options = options;
        }

        public AssemblyResult RunAssembly(Assembly assembly)
        {
            RunContext.Set(options);

            return RunTypesInternal(assembly, assembly.GetTypes());
        }

        public AssemblyResult RunNamespace(Assembly assembly, string ns)
        {
            RunContext.Set(options);

            return RunTypesInternal(assembly, assembly.GetTypes().Where(type => type.IsInNamespace(ns)).ToArray());
        }

        public AssemblyResult RunType(Assembly assembly, Type type)
        {
            RunContext.Set(options, type);

            var types = GetTypeAndNestedTypes(type).ToArray();
            return RunTypesInternal(assembly, types);
        }

        public AssemblyResult RunTypes(Assembly assembly, Convention convention, params Type[] types)
        {
            RunContext.Set(options);

            return Run(assembly, new[] { convention }, types);
        }

        public AssemblyResult RunMethods(Assembly assembly, params MethodInfo[] methods)
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

        public AssemblyResult RunMethods(Assembly assembly, MethodGroup[] methodGroups)
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

        AssemblyResult RunTypesInternal(Assembly assembly, params Type[] types)
        {
            return Run(assembly, GetConventions(assembly), types);
        }

        static Convention[] GetConventions(Assembly assembly)
        {
            return new ConventionDiscoverer(assembly).GetConventions();
        }

        AssemblyResult Run(Assembly assembly, IEnumerable<Convention> conventions, params Type[] candidateTypes)
        {
            var assemblyResult = new AssemblyResult(assembly.Location);

            listener.AssemblyStarted(assembly);

            foreach (var convention in conventions)
            {
                var conventionResult = Run(convention, candidateTypes);

                assemblyResult.Add(conventionResult);
            }

            listener.AssemblyCompleted(assembly, assemblyResult);

            return assemblyResult;
        }

        ConventionResult Run(Convention convention, Type[] candidateTypes)
        {
            var classDiscoverer = new ClassDiscoverer(convention.Config);
            var conventionResult = new ConventionResult(convention.GetType().FullName);
            var classRunner = new ClassRunner(listener, convention.Config);

            foreach (var testClass in classDiscoverer.TestClasses(candidateTypes))
            {
                var classResult = classRunner.Run(testClass);

                conventionResult.Add(classResult);
            }

            return conventionResult;
        }
    }
}