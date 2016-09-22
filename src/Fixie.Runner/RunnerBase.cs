namespace Fixie.Runner
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Execution;

    public abstract class RunnerBase
    {
        public abstract int Run(string assemblyFullPath, Options options, IReadOnlyList<string> conventionArguments);

        protected static void DiscoverMethodGroups(IReadOnlyList<Listener> listeners, string assemblyFullPath, IReadOnlyList<string> conventionArguments)
        {
            var assembly = LoadAssembly(assemblyFullPath);

            var bus = new Bus(listeners);

            Discoverer(bus, conventionArguments).DiscoverMethodGroups(assembly);
        }

        protected static void RunAssembly(IReadOnlyList<Listener> listeners, string assemblyFullPath, IReadOnlyList<string> conventionArguments)
        {
            var assembly = LoadAssembly(assemblyFullPath);

            var bus = new Bus(listeners);

            Runner(bus, conventionArguments).RunAssembly(assembly);
        }

        protected static void RunMethods(IReadOnlyList<Listener> listeners, string assemblyFullPath, IReadOnlyList<string> methodGroups,
            IReadOnlyList<string> conventionArguments)
        {
            var assembly = LoadAssembly(assemblyFullPath);

            var bus = new Bus(listeners);

            Runner(bus, conventionArguments).RunMethods(assembly, methodGroups.Select(x => new MethodGroup(x)).ToArray());
        }

        static Assembly LoadAssembly(string assemblyFullPath)
            => Assembly.Load(AssemblyName.GetAssemblyName(assemblyFullPath));

        static Runner Runner(Bus bus, IReadOnlyList<string> conventionArguments)
            => new Runner(bus, conventionArguments.ToArray());

        static Discoverer Discoverer(Bus bus, IReadOnlyList<string> conventionArguments)
            => new Discoverer(bus, conventionArguments.ToArray());
    }
}