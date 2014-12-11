using System;
using System.Collections.Generic;
using System.Reflection;
using Fixie.Discovery;
using Fixie.Internal;
using Fixie.Results;

namespace Fixie.Execution
{
    public class ExecutionProxy : MarshalByRefObject
    {
        public IReadOnlyList<MethodGroup> DiscoverTestMethodGroups(string assemblyFullPath, Lookup options)
        {
            var assembly = LoadAssembly(assemblyFullPath);

            return new Discoverer(options).DiscoverTestMethodGroups(assembly);
        }

        public AssemblyResult RunAssembly(string assemblyFullPath, Lookup options, Listener listener)
        {
            var assembly = LoadAssembly(assemblyFullPath);

            return Runner(options, listener).RunAssembly(assembly);
        }

        public AssemblyResult RunMethods(string assemblyFullPath, Lookup options, Listener listener, MethodGroup[] methodGroups)
        {
            var assembly = LoadAssembly(assemblyFullPath);

            return Runner(options, listener).RunMethods(assembly, methodGroups);
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