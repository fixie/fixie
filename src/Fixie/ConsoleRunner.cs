using System;
using System.IO;
using System.Reflection;

namespace Fixie
{
    public class ConsoleRunner : MarshalByRefObject
    {
        public Result RunAssembly(string assemblyPath)
        {
            var assemblyFullPath = Path.GetFullPath(assemblyPath);
            var assembly = Assembly.Load(AssemblyName.GetAssemblyName(assemblyFullPath));

            var runner = new Runner(new ConsoleListener());
            return runner.RunAssembly(assembly);
        }
    }
}