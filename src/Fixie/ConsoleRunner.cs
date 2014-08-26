using System;
using System.Reflection;
using Fixie.Execution;
using Fixie.Listeners;
using Fixie.Results;

namespace Fixie
{
    public class ConsoleRunner : MarshalByRefObject
    {
        public AssemblyResult RunAssembly(string assemblyFullPath, string[] args)
        {
            var assembly = Assembly.Load(AssemblyName.GetAssemblyName(assemblyFullPath));

            var options = new CommandLineParser(args).Options;

            var listener = new ListenerFactory().CreateListener(options);

            var runner = new Runner(listener, options);
            return runner.RunAssembly(assembly);
        }
    }
}