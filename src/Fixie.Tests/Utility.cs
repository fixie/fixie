namespace Fixie.Tests
{
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

        public static void Discover<TSampleTestClass>(Listener listener, Convention convention)
        {
            var sampleTestClass = typeof(TSampleTestClass);

            var bus = new Bus(listener);
            new Discoverer(bus).DiscoverMethods(sampleTestClass.Assembly, convention);
        }

        public static void Run<TSampleTestClass>(Listener listener, Convention convention)
        {
            var sampleTestClass = typeof(TSampleTestClass);

            var bus = new Bus(listener);
            new Runner(bus).RunTypes(sampleTestClass.Assembly, convention, sampleTestClass);
        }
    }
}
