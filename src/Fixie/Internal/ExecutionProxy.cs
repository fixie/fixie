using System;
using System.Collections.Generic;
using System.Reflection;
using Fixie.Execution;

namespace Fixie.Internal
{
    public class ExecutionProxy : LongLivedMarshalByRefObject
    {
        readonly Bus bus = new Bus();

        public void Subscribe(string listenerAssemblyFullPath, string listenerType, object[] listenerArgs)
        {
            var listener = CreateListener(listenerAssemblyFullPath, listenerType, listenerArgs);

            bus.Subscribe(listener);
        }

        public IReadOnlyList<MethodGroup> DiscoverTestMethodGroups(string assemblyFullPath, Options options)
        {
            var assembly = LoadAssembly(assemblyFullPath);

            return new Discoverer(options).DiscoverTestMethodGroups(assembly);
        }

        public AssemblyResult RunAssembly(string assemblyFullPath, Options options)
        {
            var runner = new Runner(bus, options);

            var assembly = LoadAssembly(assemblyFullPath);

            return runner.RunAssembly(assembly);
        }

        public AssemblyResult RunMethods(string assemblyFullPath, Options options, MethodGroup[] methodGroups)
        {
            var runner = new Runner(bus, options);

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