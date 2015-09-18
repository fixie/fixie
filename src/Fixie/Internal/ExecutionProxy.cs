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

        public AssemblyResult RunAssembly(string assemblyFullPath, string listenerFactoryAssemblyFullPath, string listenerFactoryType, Options options, IExecutionSink executionSink)
        {
            var listener = CreateListener(listenerFactoryAssemblyFullPath, listenerFactoryType, options, executionSink);

            var runner = new Runner(listener, options);

            var assembly = LoadAssembly(assemblyFullPath);

            return runner.RunAssembly(assembly);
        }

        public AssemblyResult RunMethods(string assemblyFullPath, string listenerFactoryAssemblyFullPath, string listenerFactoryType, Options options, IExecutionSink executionSink, MethodGroup[] methodGroups)
        {
            var listener = CreateListener(listenerFactoryAssemblyFullPath, listenerFactoryType, options, executionSink);

            var runner = new Runner(listener, options);

            var assembly = LoadAssembly(assemblyFullPath);

            return runner.RunMethods(assembly, methodGroups);
        }

        static Listener CreateListener(string listenerFactoryAssemblyFullPath, string listenerFactoryType, Options options, IExecutionSink executionSink)
        {
            var type = LoadAssembly(listenerFactoryAssemblyFullPath).GetType(listenerFactoryType);

            var factory = (IListenerFactory)Activator.CreateInstance(type);

            return factory.Create(options, executionSink);
        }

        static Assembly LoadAssembly(string assemblyFullPath)
        {
            return Assembly.Load(AssemblyName.GetAssemblyName(assemblyFullPath));
        }
    }
}