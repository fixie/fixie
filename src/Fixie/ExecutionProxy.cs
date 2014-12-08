using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fixie.Discovery;
using Fixie.Execution;
using Fixie.Results;

namespace Fixie
{
    public class ExecutionProxy : MarshalByRefObject
    {
        public AssemblyResult RunAssembly(string assemblyFullPath, string[] args, Listener listener)
        {
            var assembly = Assembly.Load(AssemblyName.GetAssemblyName(assemblyFullPath));

            var options = new CommandLineParser(args).Options;

            var runner = new Runner(listener, options);
            return runner.RunAssembly(assembly);
        }

        public AssemblyResult RunMethods(string assemblyFullPath, string[] args, Listener listener, TestMethod[] testMethods)
        {
            var assembly = Assembly.Load(AssemblyName.GetAssemblyName(assemblyFullPath));

            var options = new CommandLineParser(args).Options;

            var runner = new Runner(listener, options);

            var methods = testMethods.SelectMany(x => GetMethodInfo(assembly, x)).ToArray();

            return runner.RunMethods(assembly, methods);
        }

        static IEnumerable<MethodInfo> GetMethodInfo(Assembly assembly, TestMethod testMethod)
        {
            return assembly
                .GetType(testMethod.Class)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.Name == testMethod.Method);
        }
    }
}