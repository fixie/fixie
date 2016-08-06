namespace Fixie.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Execution;

    public class ExecutionProxy : LongLivedMarshalByRefObject
    {
        readonly List<Listener> subscribedListeners = new List<Listener>();

        public void Subscribe(string listenerAssemblyFullPath, string listenerType, object[] listenerArgs)
        {
            var listener = Construct<Listener>(listenerAssemblyFullPath, listenerType, listenerArgs);

            subscribedListeners.Add(listener);
        }

        static T Construct<T>(string assemblyFullPath, string typeFullName, object[] args)
        {
            var type = LoadAssembly(assemblyFullPath).GetType(typeFullName);

            return (T)Activator.CreateInstance(type, args);
        }

        public void DiscoverMethodGroups(string assemblyFullPath, Options options)
        {
            var assembly = LoadAssembly(assemblyFullPath);

            var bus = new Bus(subscribedListeners);
            new Discoverer(bus, options).DiscoverMethodGroups(assembly);
        }

        public int RunAssembly(string assemblyFullPath, Options options)
        {
            var assembly = LoadAssembly(assemblyFullPath);

            var summaryListener = new SummaryListener();
            var listeners = subscribedListeners.ToList();
            listeners.Add(summaryListener);

            var bus = new Bus(listeners);
            Runner(options, bus).RunAssembly(assembly);

            return summaryListener.Summary.Failed;
        }

        public void RunMethods(string assemblyFullPath, Options options, MethodGroup[] methodGroups)
        {
            var assembly = LoadAssembly(assemblyFullPath);

            var bus = new Bus(subscribedListeners);
            Runner(options, bus).RunMethods(assembly, methodGroups);
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