namespace Fixie.Tests
{
    using System;
    using System.Runtime.CompilerServices;
    using Fixie.Execution;

    public static class Utility
    {
        public static string FullName<T>()
            => typeof(T).FullName;

        public static string At<T>(string method, [CallerFilePath] string path = null)
            => $"   at {FullName<T>().Replace("+", ".")}.{method} in {path}:line #";

        public static string PathToThisFile([CallerFilePath] string path = null)
            => path;

        public static void Run<TSampleTestClass>(Listener listener, Convention convention)
            => Run(listener, convention, typeof(TSampleTestClass));

        public static void Run(Listener listener, Convention convention, params Type[] types)
        {
            if (types.Length == 0)
            {
                throw new InvalidOperationException("RunMany requires at least one type to be specified");
            }

            var bus = new Bus(listener);
            new Runner(bus).RunTypes(types[0].Assembly(), convention, types);
        }
    }
}
