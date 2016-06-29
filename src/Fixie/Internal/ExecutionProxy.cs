namespace Fixie.Internal
{
    using System.Reflection;
    using Execution;

    public class ExecutionProxy : LongLivedMarshalByRefObject
    {
        public void DiscoverMethodGroups(string assemblyFullPath, Options options, Bus bus)
        {
            var assembly = LoadAssembly(assemblyFullPath);

            var discoverer = new Discoverer(options);

            var methodGroups = discoverer.DiscoverMethodGroups(assembly);

            foreach (var methodGroup in methodGroups)
                bus.Publish(new MethodGroupDiscovered(methodGroup));
        }

        public void RunAssembly(string assemblyFullPath, Options options, Bus bus)
        {
            var assembly = LoadAssembly(assemblyFullPath);

            Runner(options, bus).RunAssembly(assembly);
        }

        public void RunMethods(string assemblyFullPath, Options options, Bus bus, MethodGroup[] methodGroups)
        {
            var assembly = LoadAssembly(assemblyFullPath);

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