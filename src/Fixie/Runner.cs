using System;
using System.IO;
using System.Reflection;

namespace Fixie
{
    public class Runner : MarshalByRefObject
    {
        readonly string assemblyPath;
        readonly Listener listener;

        public Runner(string assemblyPath)
            : this(assemblyPath, new ConsoleListener())
        {
        }

        public Runner(string assemblyPath, Listener listener)
        {
            this.assemblyPath = assemblyPath;
            this.listener = listener;
        }

        public Result Execute()
        {
            var assemblyFullPath = Path.GetFullPath(assemblyPath);
            var assembly = Assembly.Load(AssemblyName.GetAssemblyName(assemblyFullPath));
            var convention = new DefaultConvention();
            var suite = new Suite(convention, assembly.GetTypes());
            var result = suite.Execute(listener);
            return result;
        }
    }
}