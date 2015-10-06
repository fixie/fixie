using System;
using System.Collections.Generic;
using System.Linq;
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

        public AssemblyResult RunAssembly(string assemblyFullPath, string listenerFactoryAssemblyFullPath, string listenerFactoryType, Options options, object[] listenerFactoryArgs)
        {
            var listener = CreateListener(listenerFactoryAssemblyFullPath, listenerFactoryType, options, listenerFactoryArgs);

            var runner = new Runner(listener, options);

            var assembly = LoadAssembly(assemblyFullPath);

            var assemblyResult = runner.RunAssembly(assembly);

            var sink = ((IExecutionSink)listenerFactoryArgs.First());
            sink.SendMessage("------------");
            sink.SendMessage("Although passed across as simply an object, the execution sink's runtime type in the child domain is: " + sink.GetType());
            sink.SendMessage("At the end of this run, the child appdomain contained these assemblies:");
            foreach (var a in AppDomain.CurrentDomain.GetAssemblies().Select(x => x.ToString()).OrderBy(x => x))
                sink.SendMessage("\t" + a.ToString());
            sink.SendMessage("------------");

            return assemblyResult;
        }

        public AssemblyResult RunMethods(string assemblyFullPath, string listenerFactoryAssemblyFullPath, string listenerFactoryType, Options options, MethodGroup[] methodGroups, object[] listenerFactoryArgs)
        {
            var listener = CreateListener(listenerFactoryAssemblyFullPath, listenerFactoryType, options, listenerFactoryArgs);

            var runner = new Runner(listener, options);

            var assembly = LoadAssembly(assemblyFullPath);

            var assemblyResult = runner.RunMethods(assembly, methodGroups);

            var sink = ((IExecutionSink)listenerFactoryArgs.First());
            sink.SendMessage("------------");
            sink.SendMessage("Although passed across as simply an object, the execution sink's runtime type in the child domain is: " + sink.GetType());
            sink.SendMessage("At the end of this run, the child appdomain contained these assemblies:");
            foreach (var a in AppDomain.CurrentDomain.GetAssemblies().Select(x => x.ToString()).OrderBy(x => x))
                sink.SendMessage("\t" + a.ToString());
            sink.SendMessage("------------");

            return assemblyResult;
        }

        static Listener CreateListener(string listenerFactoryAssemblyFullPath, string listenerFactoryType, Options options, object[] listenerFactoryArgs)
        {
            var type = LoadAssembly(listenerFactoryAssemblyFullPath).GetType(listenerFactoryType);

            var factory = (IListenerFactory)Activator.CreateInstance(type, listenerFactoryArgs);

            return factory.Create(options);
        }

        static Assembly LoadAssembly(string assemblyFullPath)
        {
            return Assembly.Load(AssemblyName.GetAssemblyName(assemblyFullPath));
        }
    }
}