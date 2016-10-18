namespace Fixie.Runner
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Execution;

    public abstract class RunnerBase
    {
        public abstract int Run(string assemblyFullPath, Assembly assembly, Options options, IReadOnlyList<string> conventionArguments);

        protected static void DiscoverMethods(Assembly assembly, IReadOnlyList<string> conventionArguments, IReadOnlyList<Listener> listeners)
        {
            var bus = new Bus(listeners);

            Discoverer(bus, conventionArguments).DiscoverMethods(assembly);
        }

        protected static void RunAssembly(Assembly assembly, IReadOnlyList<string> conventionArguments, IReadOnlyList<Listener> listeners)
        {
            var bus = new Bus(listeners);

            Runner(bus, conventionArguments).RunAssembly(assembly);
        }

        protected static void RunMethods(Assembly assembly, IReadOnlyList<string> conventionArguments, IReadOnlyList<string> methodGroups, IReadOnlyList<Listener> listeners)
        {
            var bus = new Bus(listeners);

            var methods = methodGroups
                .Select(x => new MethodGroup(x))
                .SelectMany(methodGroup => GetMethods(assembly, methodGroup))
                .ToArray();

            Runner(bus, conventionArguments).RunMethods(assembly, methods);
        }

        static Runner Runner(Bus bus, IReadOnlyList<string> conventionArguments)
            => new Runner(bus, conventionArguments.ToArray());

        static Discoverer Discoverer(Bus bus, IReadOnlyList<string> conventionArguments)
            => new Discoverer(bus, conventionArguments.ToArray());

        static IEnumerable<Method> GetMethods(Assembly assembly, MethodGroup methodGroup)
        {
            var testClass = assembly.GetType(methodGroup.Class);

            return testClass
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.Name == methodGroup.Method)
                .Select(m => new Method(testClass, m));
        }
    }
}