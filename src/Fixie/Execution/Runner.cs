using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fixie.Discovery;
using Fixie.Results;

namespace Fixie.Execution
{
    public class Runner
    {
        public static MethodInfo[] GetMethods(MethodGroup[] methodGroups, Assembly assembly)
        {
            return methodGroups.SelectMany(x => GetMethodInfo(assembly, x)).ToArray();
        }

        static IEnumerable<MethodInfo> GetMethodInfo(Assembly assembly, MethodGroup methodGroup)
        {
            return assembly
                .GetType(methodGroup.Class)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.Name == methodGroup.Method);
        }

        readonly Listener listener;
        readonly Lookup options;

        public Runner(Listener listener)
            : this(listener, new Lookup()) { }

        public Runner(Listener listener, Lookup options)
        {
            this.listener = listener;
            this.options = options;
        }

        public AssemblyResult RunAssembly(Assembly assembly)
        {
            var runContext = new RunContext(assembly, options);

            return RunTypes(runContext, assembly.GetTypes());
        }

        public AssemblyResult RunNamespace(Assembly assembly, string ns)
        {
            var runContext = new RunContext(assembly, options);

            return RunTypes(runContext, assembly.GetTypes().Where(type => type.IsInNamespace(ns)).ToArray());
        }

        public AssemblyResult RunType(Assembly assembly, Type type)
        {
            var runContext = new RunContext(assembly, options, type);

            return RunTypes(runContext, type);
        }

        public AssemblyResult RunTypes(Assembly assembly, Convention convention, params Type[] types)
        {
            var runContext = new RunContext(assembly, options);

            return RunTypes(runContext, convention, types);
        }

        public AssemblyResult RunMethod(Assembly assembly, MethodInfo method)
        {
            var runContext = new RunContext(assembly, options, method);

            var conventions = GetConventions(runContext);

            foreach (var convention in conventions)
                convention.Methods.Where(m => m == method);

            var type = method.DeclaringType;

            return Run(runContext, conventions, type);
        }

        public AssemblyResult RunMethods(Assembly assembly, params MethodInfo[] methods)
        {
            if (methods.Length == 1)
                return RunMethod(assembly, methods.Single());

            var runContext = new RunContext(assembly, options);

            var conventions = GetConventions(runContext);

            foreach (var convention in conventions)
                convention.Methods.Where(methods.Contains);

            return Run(runContext, conventions, methods.Select(m => m.ReflectedType).Distinct().ToArray());
        }

        private AssemblyResult RunTypes(RunContext runContext, params Type[] types)
        {
            return Run(runContext, GetConventions(runContext), types);
        }

        private AssemblyResult RunTypes(RunContext runContext, Convention convention, params Type[] types)
        {
            return Run(runContext, new[] { convention }, types);
        }

        private static Convention[] GetConventions(RunContext runContext)
        {
            return new ConventionDiscoverer(runContext).GetConventions();
        }

        AssemblyResult Run(RunContext runContext, IEnumerable<Convention> conventions, params Type[] candidateTypes)
        {
            var assemblyResult = new AssemblyResult(runContext.Assembly.Location);
            var assemblyInfo = new AssemblyInfo(runContext.Assembly);

            listener.AssemblyStarted(assemblyInfo);

            foreach (var convention in conventions)
            {
                var conventionResult = Run(convention, candidateTypes);

                assemblyResult.Add(conventionResult);
            }

            listener.AssemblyCompleted(assemblyInfo, assemblyResult);

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