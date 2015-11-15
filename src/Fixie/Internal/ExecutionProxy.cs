using System;
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

        public AssemblyResult RunAssembly(string assemblyFullPath, string listenerAssemblyFullPath, string listenerType, Options options, object[] listenerArgs)
        {
            var listener = CreateListener(listenerAssemblyFullPath, listenerType, listenerArgs);

            var runner = new Runner(listener, options);

            var assembly = LoadAssembly(assemblyFullPath);

            return runner.RunAssembly(assembly);
        }

        public AssemblyResult RunMethods(string assemblyFullPath, string listenerAssemblyFullPath, string listenerType, Options options, MethodGroup[] methodGroups, object[] listenerArgs)
        {
            var listener = CreateListener(listenerAssemblyFullPath, listenerType, listenerArgs);

            var runner = new Runner(listener, options);

            var assembly = LoadAssembly(assemblyFullPath);

            return runner.RunMethods(assembly, methodGroups);
        }

        static Listener CreateListener(string listenerAssemblyFullPath, string listenerType, object[] listenerArgs)
        {
            var type = LoadAssembly(listenerAssemblyFullPath).GetType(listenerType);

            var listener = (Listener)Activator.CreateInstance(type, listenerArgs);

            return listener;
        }

        static Assembly LoadAssembly(string assemblyFullPath)
        {
            return Assembly.Load(AssemblyName.GetAssemblyName(assemblyFullPath));
        }
    }
}