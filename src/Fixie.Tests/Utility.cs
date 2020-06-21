namespace Fixie.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Fixie.Internal;

    public static class Utility
    {
        public static string FullName<T>()
        {
            return typeof(T).FullName ??
                   throw new Exception($"Expected type {typeof(T).Name} to have a non-null FullName.");
        }

        public static string At<T>(string method, [CallerFilePath] string path = default!)
            => $"   at {FullName<T>().Replace("+", ".")}.{method} in {path}:line #";

        public static string[] For<TSampleTestClass>(params string[] entries)
            => entries.Select(x => FullName<TSampleTestClass>() + x).ToArray();

        public static string PathToThisFile([CallerFilePath] string path = default!)
            => path;

        public static IEnumerable<string> Run<TSampleTestClass>(Discovery discovery, Execution execution)
        {
            var listener = new StubListener();
            Run(listener, discovery, execution, typeof(TSampleTestClass));
            return listener.Entries;
        }

        public static IEnumerable<string> Run<TSampleTestClass>()
            => Run<TSampleTestClass>(new SelfTestDiscovery(), new DefaultExecution());

        public static void Discover(Listener listener, Discovery discovery, params Type[] candidateTypes)
        {
            if (candidateTypes.Length == 0)
                throw new InvalidOperationException("At least one type must be specified.");

            var bus = new Bus(listener);
            new AssemblyRunner(candidateTypes[0].Assembly, bus).DiscoverMethods(candidateTypes, discovery);
        }

        public static void Run(Listener listener, Discovery discovery, Execution execution, params Type[] candidateTypes)
        {
            if (candidateTypes.Length == 0)
                throw new InvalidOperationException("At least one type must be specified.");

            var bus = new Bus(listener);
            new AssemblyRunner(candidateTypes[0].Assembly, bus).Run(candidateTypes, discovery, execution);
        }
    }
}
