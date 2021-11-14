namespace Fixie.Tests;

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
    public static TestEnvironment GetTestEnvironment()
    {
        return new TestEnvironment(
            typeof(TestProject).Assembly,
            System.Console.Out,
            Directory.GetCurrentDirectory());
    }

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

    public static async Task<IEnumerable<string>> Run(Type testClass, IExecution execution)
    {
        var report = new StubReport();
        var discovery = new SelfTestDiscovery();

        await Run(report, discovery, execution, testClass);
        return report.Entries;
    }

    public static async Task<IEnumerable<string>> Run(Type[] testClasses, IExecution execution)
    {
        var report = new StubReport();
        var discovery = new SelfTestDiscovery();

        await Run(report, discovery, execution, testClasses);
        return report.Entries;
    }

    public static async Task Discover(IReport report, IDiscovery discovery, params Type[] candidateTypes)
    {
        if (candidateTypes.Length == 0)
            throw new InvalidOperationException("At least one type must be specified.");

        var environment = new TestEnvironment(candidateTypes[0].Assembly, System.Console.Out, Directory.GetCurrentDirectory());
        var runner = new Runner(environment, report);

        await runner.Discover(candidateTypes, discovery);
    }

    internal static async Task Run(IReport report, IDiscovery discovery, IExecution execution, params Type[] candidateTypes)
    {
        if (candidateTypes.Length == 0)
            throw new InvalidOperationException("At least one type must be specified.");

        var environment = new TestEnvironment(candidateTypes[0].Assembly, System.Console.Out, Directory.GetCurrentDirectory());
        var runner = new Runner(environment, report);
        var configuration = new TestConfiguration();
        configuration.Conventions.Add(discovery, execution);

        await runner.Run(candidateTypes, configuration, ImmutableHashSet<string>.Empty);
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