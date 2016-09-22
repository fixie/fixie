namespace Fixie.Runner
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Execution;

    public abstract class RunnerBase
    {
        public abstract int Run(string assemblyFullPath, Options options, IReadOnlyList<string> conventionArguments);

        protected static void DiscoverMethodGroups(Assembly assembly, IReadOnlyList<string> conventionArguments, IReadOnlyList<Listener> listeners)
        {
            var bus = new Bus(listeners);

            Discoverer(bus, conventionArguments).DiscoverMethodGroups(assembly);
        }

        protected static void RunAssembly(Assembly assembly, IReadOnlyList<string> conventionArguments, IReadOnlyList<Listener> listeners)
        {
            var bus = new Bus(listeners);

            Runner(bus, conventionArguments).RunAssembly(assembly);
        }

        protected static void RunMethods(Assembly assembly, IReadOnlyList<string> conventionArguments, IReadOnlyList<string> methodGroups, IReadOnlyList<Listener> listeners)
        {
            var bus = new Bus(listeners);

            Runner(bus, conventionArguments).RunMethods(assembly, methodGroups.Select(x => new MethodGroup(x)).ToArray());
        }

        static Runner Runner(Bus bus, IReadOnlyList<string> conventionArguments)
            => new Runner(bus, conventionArguments.ToArray());

        static Discoverer Discoverer(Bus bus, IReadOnlyList<string> conventionArguments)
            => new Discoverer(bus, conventionArguments.ToArray());
    }
}