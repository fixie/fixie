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
            => typeof(T).FullName;

        public static string At<T>(string method, [CallerFilePath] string path = null)
            => $"   at {FullName<T>().Replace("+", ".")}.{method} in {path}:line #";

        public static string[] For<TSampleTestClass>(params string[] entries)
            => entries.Select(x => FullName<TSampleTestClass>() + x).ToArray();

        public static string PathToThisFile([CallerFilePath] string path = null)
            => path;

        public static IEnumerable<string> Run<TSampleTestClass>(Discovery discovery, Lifecycle lifecycle)
        {
            var listener = new StubListener();
            RunTypes(listener, discovery, lifecycle, typeof(TSampleTestClass));
            return listener.Entries;
        }

        public static IEnumerable<string> Run<TSampleTestClass>()
            => Run<TSampleTestClass>(new SelfTestDiscovery(), new DefaultLifecycle());

        public static void RunTypes(Listener listener, Discovery discovery, Lifecycle lifecycle, params Type[] types)
        {
            if (types.Length == 0)
            {
                throw new InvalidOperationException("RunTypes requires at least one type to be specified");
            }

            var bus = new Bus(listener);
            new Runner(bus).RunTypes(types[0].Assembly, discovery, lifecycle, types);
        }
    }
}
