using System;
using System.Collections.Generic;
using System.Reflection;
using Fixie.Discovery;

namespace Fixie
{
    public class DiscoveryProxy : MarshalByRefObject
    {
        public IReadOnlyList<MethodGroup> TestMethodGroups(string assemblyFullPath, Lookup options)
        {
            var assembly = LoadAssembly(assemblyFullPath);

            return new Discoverer(options).TestMethodGroups(assembly);
        }

        static Assembly LoadAssembly(string assemblyFullPath)
        {
            return Assembly.Load(AssemblyName.GetAssemblyName(assemblyFullPath));
        }
    }
}