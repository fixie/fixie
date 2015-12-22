using System;
using System.Collections.Generic;
using System.Reflection;
using Fixie.Execution;

namespace Fixie.Internal
{
    public class ExecutionProxy : LongLivedMarshalByRefObject
    {
        readonly List<object> subscribers = new List<object>();

        public void Subscribe<TListener>(TListener listener) where TListener : LongLivedMarshalByRefObject
        {
            subscribers.Add(listener);
        }

        public void Subscribe(string listenerAssemblyFullPath, string listenerType, object[] listenerArgs)
        {
            var assembly = LoadAssembly(listenerAssemblyFullPath);

            var type = assembly.GetType(listenerType);

            var listener = Activator.CreateInstance(type, listenerArgs);

            subscribers.Add(listener);
        }

        public void DiscoverMethodGroups(string assemblyFullPath, Options options)
        {
            var bus = CreateBus();

            var assembly = LoadAssembly(assemblyFullPath);

            var discoverer = new Discoverer(options);

            var methodGroups = discoverer.DiscoverTestMethodGroups(assembly);

            foreach (var methodGroup in methodGroups)
                bus.Publish(new MethodGroupDiscovered(methodGroup));
        }

        public ExecutionSummary RunAssembly(string assemblyFullPath, Options options)
        {
            var bus = CreateBus();

            var summary = new ExecutionSummary();
            bus.Subscribe(new ExecutionSummaryListener(summary));

            var runner = new Runner(bus, options);

            runner.RunAssembly(LoadAssembly(assemblyFullPath));
            return summary;
        }

        public ExecutionSummary RunMethods(string assemblyFullPath, Options options, MethodGroup[] methodGroups)
        {
            var bus = CreateBus();

            var summary = new ExecutionSummary();
            bus.Subscribe(new ExecutionSummaryListener(summary));

            var runner = new Runner(bus, options);
            runner.RunMethods(LoadAssembly(assemblyFullPath), methodGroups);

            return summary;
        }

        Bus CreateBus()
        {
            var bus = new Bus();

            foreach (var subscriber in subscribers)
                bus.Subscribe(subscriber);

            return bus;
        }

        static Assembly LoadAssembly(string assemblyFullPath)
        {
            return Assembly.Load(AssemblyName.GetAssemblyName(assemblyFullPath));
        }

        class ExecutionSummaryListener : IHandler<CaseCompleted>
        {
            readonly ExecutionSummary summary;

            public ExecutionSummaryListener(ExecutionSummary summary)
            {
                this.summary = summary;
            }

            public void Handle(CaseCompleted message)
                => summary.Include(message);
        }
    }
}