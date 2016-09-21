namespace Fixie.Runner
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Execution;

    public class ExecutionProxy : LongLivedMarshalByRefObject
    {
        readonly List<Listener> subscribedListeners = new List<Listener>();

        public void Subscribe(string listenerAssemblyFullPath, string listenerType, object[] listenerArguments)
        {
            var listener = Construct<Listener>(listenerAssemblyFullPath, listenerType, listenerArguments);

            subscribedListeners.Add(listener);
        }

        static T Construct<T>(string assemblyFullPath, string typeFullName, object[] constructorArguments)
        {
            var type = LoadAssembly(assemblyFullPath).GetType(typeFullName);

            return (T)Activator.CreateInstance(type, constructorArguments);
        }

        public void DiscoverMethodGroups(string assemblyFullPath, string[] conventionArguments)
        {
            var assembly = LoadAssembly(assemblyFullPath);

            var bus = new Bus(subscribedListeners);
            Discoverer(bus, conventionArguments).DiscoverMethodGroups(assembly);
        }

        public int RunAssembly(string assemblyFullPath, string[] conventionArguments)
        {
            var assembly = LoadAssembly(assemblyFullPath);

            var summaryListener = new SummaryListener();
            var listeners = subscribedListeners.ToList();
            listeners.Add(summaryListener);

            var bus = new Bus(listeners);
            Runner(bus, conventionArguments).RunAssembly(assembly);

            return summaryListener.Summary.Failed;
        }

        public void RunMethods(string assemblyFullPath, IReadOnlyList<string> methodGroups, string[] conventionArguments)
        {
            var assembly = LoadAssembly(assemblyFullPath);

            var bus = new Bus(subscribedListeners);

            Runner(bus, conventionArguments).RunMethods(assembly, methodGroups.Select(x => new MethodGroup(x)).ToArray());
        }

        static Assembly LoadAssembly(string assemblyFullPath)
        {
            return Assembly.Load(AssemblyName.GetAssemblyName(assemblyFullPath));
        }

        static Runner Runner(Bus bus, string[] conventionArguments)
            => new Runner(bus, conventionArguments);

        static Discoverer Discoverer(Bus bus, string[] conventionArguments)
            => new Discoverer(bus, conventionArguments);
    }
}