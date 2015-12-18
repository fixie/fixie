using System;
using System.Collections.Generic;
using System.Reflection;
using Fixie.Execution;

namespace Fixie.Internal
{
    public class ExecutionProxy : LongLivedMarshalByRefObject
    {
        readonly Bus bus = new Bus();

        internal void Subscribe<TListener>(TListener listener) where TListener : LongLivedMarshalByRefObject
        {
            bus.Subscribe(listener);
        }

        internal void Subscribe(string listenerAssemblyFullPath, string listenerType, object[] listenerArgs)
        {
            var assembly = LoadAssembly(listenerAssemblyFullPath);

            var type = assembly.GetType(listenerType);

            var listener = Activator.CreateInstance(type, listenerArgs);

            bus.Subscribe(listener);
        }

        internal IReadOnlyList<MethodGroup> DiscoverTestMethodGroups(string assemblyFullPath, Options options)
        {
            var assembly = LoadAssembly(assemblyFullPath);

            var discoverer = new Discoverer(options);

            return discoverer.DiscoverTestMethodGroups(assembly);
        }

        internal AssemblyReport RunAssembly(string assemblyFullPath, Options options)
        {
            var assembly = LoadAssembly(assemblyFullPath);

            var runner = new Runner(bus, options);

            return runner.RunAssembly(assembly);
        }

        internal AssemblyReport RunMethods(string assemblyFullPath, Options options, MethodGroup[] methodGroups)
        {
            var assembly = LoadAssembly(assemblyFullPath);

            var runner = new Runner(bus, options);

            return runner.RunMethods(assembly, methodGroups);
        }

        static Assembly LoadAssembly(string assemblyFullPath)
        {
            return Assembly.Load(AssemblyName.GetAssemblyName(assemblyFullPath));
        }
    }
}