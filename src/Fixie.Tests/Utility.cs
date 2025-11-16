using System.Runtime.CompilerServices;
using Fixie.Internal;
using Fixie.Reports;

namespace Fixie.Tests;

public static class Utility
{
    public static TestEnvironment GetTestEnvironment(TextWriter console) =>
        new(typeof(TestProject).Assembly, null, console, customArguments: []);

    public const string TargetFrameworkVersion =
        #if NET9_0
        "9.0"
        #elif NET10_0
        "10.0"
        #endif
        ;

    public static string FullName<T>()
    {
        return typeof(T).FullName ??
               throw new Exception($"Expected type {typeof(T).Name} to have a non-null FullName.");
    }

    public static string At<T>(string method, [CallerFilePath] string path = default!)
        => At(typeof(T), method, path);

    public static string At(Type type, string method, string[] relativePathFromCallingCodeFile, [CallerFilePath] string path = default!)
    {
        var absolutePath = Path.GetFullPath(
            path: Path.Join(relativePathFromCallingCodeFile),
            basePath: Path.GetDirectoryName(path)!);

        return At(type, method, absolutePath);
    }

    static string At(Type type, string method, string path)
    {
        var typeFullName = type.FullName ??
                           throw new Exception($"Expected type {type.Name} to have a non-null FullName.");

        return $"   at {typeFullName.Replace("+", ".")}.{method} in {path}:line #";
    }

    public static async Task<IEnumerable<string>> Run(Type testClass, IExecution execution, TextWriter console)
    {
        var report = new StubReport();
        var discovery = new SelfTestDiscovery();

        await Run(report, discovery, execution, console, testClass);
        return report.Entries;
    }

    public static async Task<IEnumerable<string>> Run(Type[] testClasses, IExecution execution, TextWriter console)
    {
        var report = new StubReport();
        var discovery = new SelfTestDiscovery();

        await Run(report, discovery, execution, console, testClasses);
        return report.Entries;
    }

    internal static async Task Run(IReport report, IDiscovery discovery, IExecution execution, TextWriter console, params Type[] candidateTypes)
    {
        if (candidateTypes.Length == 0)
            throw new InvalidOperationException("At least one type must be specified.");

        var environment = GetTestEnvironment(console);
        var runner = new Runner(environment, report);
        var configuration = new TestConfiguration();
        configuration.Conventions.Add(discovery, execution);

        await runner.Run(candidateTypes, configuration, []);
    }

    public static IEnumerable<object?[]> FromInputAttributes(Test test)
    {
        return test.HasParameters
            ? test.GetAll<InputAttribute>().Select(x => x.Parameters)
            : InvokeOnceWithZeroParameters;
    }

    static readonly object[] EmptyParameters = [];
    static readonly object[][] InvokeOnceWithZeroParameters = [EmptyParameters];
}