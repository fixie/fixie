using System;
using System.Reflection;
using Fixie.Discovery;
using Fixie.Execution;
using Fixie.Results;

namespace Fixie
{
    public class ExecutionProxy : MarshalByRefObject
    {
        public AssemblyResult RunAssembly(string assemblyFullPath, Lookup options, Listener listener)
        {
            var assembly = LoadAssembly(assemblyFullPath);

            return Runner(options, listener).RunAssembly(assembly);
        }

        public AssemblyResult RunMethods(string assemblyFullPath, Lookup options, Listener listener, MethodGroup[] methodGroups)
        {
            var assembly = LoadAssembly(assemblyFullPath);

            var methods = Execution.Runner.GetMethods(methodGroups, assembly);

            return Runner(options, listener).RunMethods(assembly, methods);
        }

        static Assembly LoadAssembly(string assemblyFullPath)
        {
            return Assembly.Load(AssemblyName.GetAssemblyName(assemblyFullPath));
        }

        static Runner Runner(Lookup options, Listener listener)
        {
            return new Runner(listener, options);
        }
    }
}