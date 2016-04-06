using System.Collections.Generic;
using System.Reflection;
using Fixie.Execution;

namespace Fixie.Internal
{
    public class ExecutionProxy : LongLivedMarshalByRefObject
    {
        public IReadOnlyList<MethodGroup> DiscoverTestMethodGroups(string assemblyFullPath, Options options)
        {
            var assembly = LoadAssembly(assemblyFullPath);

            return new Discoverer(options).DiscoverTestMethodGroups(assembly);
        }

        public AssemblyResult RunAssembly(string assemblyFullPath, Options options, Bus bus)
        {
            var assembly = LoadAssembly(assemblyFullPath);

            return Runner(options, bus).RunAssembly(assembly);
        }

        public AssemblyResult RunMethods(string assemblyFullPath, Options options, Bus bus, MethodGroup[] methodGroups)
        {
            var assembly = LoadAssembly(assemblyFullPath);

            return Runner(options, bus).RunMethods(assembly, methodGroups);
        }

        static Assembly LoadAssembly(string assemblyFullPath)
        {
            return Assembly.Load(AssemblyName.GetAssemblyName(assemblyFullPath));
        }

        static Runner Runner(Options options, Bus bus)
        {
            return new Runner(bus, options);
        }
    }
}