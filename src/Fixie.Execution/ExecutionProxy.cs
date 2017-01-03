namespace Fixie.Execution
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public class ExecutionProxy : LongLivedMarshalByRefObject
    {
        readonly List<Listener> listeners = new List<Listener>();

        public void RegisterListener<TListener>(object[] listenerArguments) where TListener : Listener
        {
            listeners.Add((Listener)Activator.CreateInstance(typeof(TListener), listenerArguments));
        }

        public IReadOnlyList<MethodGroup> DiscoverTestMethodGroups(string assemblyFullPath, Options options)
        {
            var assembly = LoadAssembly(assemblyFullPath);

            return new Discoverer(options).DiscoverTestMethodGroups(assembly);
        }

        public AssemblyReport RunAssembly(string assemblyFullPath, Options options)
        {
            var assembly = LoadAssembly(assemblyFullPath);

            return Runner(options).RunAssembly(assembly);
        }

        public AssemblyReport RunMethods(string assemblyFullPath, Options options, MethodGroup[] methodGroups)
        {
            var assembly = LoadAssembly(assemblyFullPath);

            return Runner(options).RunMethods(assembly, methodGroups);
        }

        static Assembly LoadAssembly(string assemblyFullPath)
        {
            return Assembly.Load(AssemblyName.GetAssemblyName(assemblyFullPath));
        }

        Runner Runner(Options options)
        {
            var bus = new Bus(listeners);
            return new Runner(bus, options);
        }
    }
}