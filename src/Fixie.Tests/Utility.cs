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

        public static void Run<TSampleTestClass>(Listener listener, Convention convention)
        {
            var sampleTestClass = typeof(TSampleTestClass);

            var bus = new Bus(listener);
            new Runner(bus).RunTypes(sampleTestClass.Assembly(), convention, sampleTestClass);
        }

        public static void RunMany<TSampleTestClass1, TSampleTestClass2>(Listener listener, Convention convention)
        {
            var sampleTestClass1 = typeof(TSampleTestClass1);
            var sampleTestClass2 = typeof(TSampleTestClass2);

            var types = new[] {sampleTestClass1, sampleTestClass2};

            var bus = new Bus(listener);
            new Runner(bus).RunTypes(sampleTestClass1.Assembly(), convention, types);
        }
    }
}
