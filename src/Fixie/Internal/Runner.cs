using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fixie.Execution;

namespace Fixie.Internal
{
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

        public ExecutionSummary RunAssembly(Assembly assembly)
        {
            RunContext.Set(options);

            return RunTypesInternal(assembly, assembly.GetTypes());
        }

        public ExecutionSummary RunNamespace(Assembly assembly, string ns)
        {
            RunContext.Set(options);

            return RunTypesInternal(assembly, assembly.GetTypes().Where(type => type.IsInNamespace(ns)).ToArray());
        }

        public ExecutionSummary RunType(Assembly assembly, Type type)
        {
            RunContext.Set(options, type);

            var types = GetTypeAndNestedTypes(type).ToArray();
            return RunTypesInternal(assembly, types);
        }

        public ExecutionSummary RunTypes(Assembly assembly, Convention convention, params Type[] types)
        {
            RunContext.Set(options);

            return Run(assembly, new[] { convention }, types);
        }

        public ExecutionSummary RunMethods(Assembly assembly, params MethodInfo[] methods)
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

        public ExecutionSummary RunMethods(Assembly assembly, MethodGroup[] methodGroups)
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

        ExecutionSummary RunTypesInternal(Assembly assembly, params Type[] types)
        {
            return Run(assembly, GetConventions(assembly), types);
        }

        static Convention[] GetConventions(Assembly assembly)
        {
            return new ConventionDiscoverer(assembly).GetConventions();
        }

        ExecutionSummary Run(Assembly assembly, IEnumerable<Convention> conventions, params Type[] candidateTypes)
        {
            var summary = new ExecutionSummary();

            bus.Publish(new AssemblyStarted(assembly));

            foreach (var convention in conventions)
                Run(summary, convention, candidateTypes);

            bus.Publish(new AssemblyCompleted(assembly, summary));

            return summary;
        }

        void Run(ExecutionSummary summary, Convention convention, Type[] candidateTypes)
        {
            var classDiscoverer = new ClassDiscoverer(convention.Config);
            var classRunner = new ClassRunner(bus, convention.Config);

            foreach (var testClass in classDiscoverer.TestClasses(candidateTypes))
            {
                var classReport = classRunner.Run(testClass);

                summary.Duration += classReport.Duration;
                summary.Passed += classReport.Passed;
                summary.Failed += classReport.Failed;
                summary.Skipped += classReport.Skipped;
            }
        }
    }
}