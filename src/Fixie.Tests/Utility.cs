namespace Fixie.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text.RegularExpressions;
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
            => At(typeof(T), method, NormalizedPath(path));

        public static string At(Type type, string method, string normalizedPath)
        {
            var typeFullName = type.FullName ??
                   throw new Exception($"Expected type {type.Name} to have a non-null FullName.");

            return $"   at {typeFullName.Replace("+", ".")}.{method} in {normalizedPath}:line #";
        }

        public static string NormalizedPath(string path)
            => Regex.Replace(path,
                @".+([\\/])src([\\/])Fixie(.+)\.cs",
                "...$1src$2Fixie$3.cs");

        public static string PathToThisFile([CallerFilePath] string path = default!)
            => path;

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

            var context = new TestContext(candidateTypes[0].Assembly, System.Console.Out, Directory.GetCurrentDirectory());
            var runner = new Runner(context, report);

            await runner.DiscoverAsync(candidateTypes, discovery);
        }

        public static async Task RunAsync(Report report, Discovery discovery, Execution execution, params Type[] candidateTypes)
        {
            if (candidateTypes.Length == 0)
                throw new InvalidOperationException("At least one type must be specified.");

            var context = new TestContext(candidateTypes[0].Assembly, System.Console.Out, Directory.GetCurrentDirectory());
            var runner = new Runner(context, report);

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
