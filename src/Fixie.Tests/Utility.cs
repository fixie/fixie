namespace Fixie.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Fixie.Internal;
    using Fixie.Reports;

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

        public static Task<IEnumerable<string>> RunAsync<TSampleTestClass>()
            => RunAsync<TSampleTestClass, DefaultExecution>();

        public static Task<IEnumerable<string>> RunAsync<TSampleTestClass, TExecution>() where TExecution : Execution, new()
            => RunAsync<TSampleTestClass>(new TExecution());

        public static Task<IEnumerable<string>> RunAsync<TSampleTestClass>(Execution execution)
            => RunAsync(typeof(TSampleTestClass), execution);

        public static Task<IEnumerable<string>> RunAsync<TExecution>(Type testClass) where TExecution : Execution, new()
            => RunAsync(testClass, new TExecution());

        public static async Task<IEnumerable<string>> RunAsync(Type testClass, Execution execution)
        {
            var report = new StubReport();
            var discovery = new SelfTestDiscovery();
            await RunAsync(report, discovery, execution, testClass);
            return report.Entries;
        }

        public static async Task<IEnumerable<string>> RunAsync(Type[] testClasses, Execution execution)
        {
            var report = new StubReport();
            var discovery = new SelfTestDiscovery();
            await RunAsync(report, discovery, execution, testClasses);
            return report.Entries;
        }

        public static async Task DiscoverAsync(Report report, Discovery discovery, params Type[] candidateTypes)
        {
            if (candidateTypes.Length == 0)
                throw new InvalidOperationException("At least one type must be specified.");

            var environment = new TestEnvironment(candidateTypes[0].Assembly, System.Console.Out);
            var runner = new Runner(environment, report);

            await runner.DiscoverAsync(candidateTypes, discovery);
        }

        public static async Task RunAsync(Report report, Discovery discovery, Execution execution, params Type[] candidateTypes)
        {
            if (candidateTypes.Length == 0)
                throw new InvalidOperationException("At least one type must be specified.");

            var environment = new TestEnvironment(candidateTypes[0].Assembly, System.Console.Out);
            var runner = new Runner(environment, report);

            await runner.RunAsync(candidateTypes, discovery, execution, ImmutableHashSet<string>.Empty);
        }

        public static IEnumerable<object?[]> FromInputAttributes(Test test)
        {
            return test.HasParameters
                ? test.GetAll<InputAttribute>().Select(x => x.Parameters)
                : InvokeOnceWithZeroParameters;
        }

        static readonly object[] EmptyParameters = {};
        static readonly object[][] InvokeOnceWithZeroParameters = { EmptyParameters };
    }
}
