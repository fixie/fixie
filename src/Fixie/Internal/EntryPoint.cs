using System.Reflection;
using Fixie.Reports;
using static System.Environment;
using static Fixie.Internal.Maybe;

namespace Fixie.Internal;

public class EntryPoint
{
    enum ExitCode
    {
        Success = 0,
        Failure = 1,
        FatalError = 2
    }

    public static async Task<int> Main(Assembly assembly, ITestProject? testProject, string[] customArguments)
    {
        SetEnvironmentVariable("TESTINGPLATFORM_TELEMETRY_OPTOUT", "true");

        var console = Console.Out;
        var targetFramework = GetEnvironmentVariable("FIXIE_TARGET_FRAMEWORK");
        var environment = new TestEnvironment(assembly, targetFramework, console, customArguments);

        using var boundary = new ConsoleRedirectionBoundary();

        try
        {
            var reports = DefaultReports(environment).ToArray();

            var pattern = GetEnvironmentVariable("FIXIE_TESTS_PATTERN");

            return pattern == null
                ? (int) await Run(environment, testProject, reports, async runner => await runner.Run())
                : (int) await Run(environment, testProject, reports, async runner => await runner.Run(new TestPattern(pattern)));
        }
        catch (Exception exception)
        {
            using (Foreground.Red)
                console.WriteLine($"Fatal Error: {exception}");

            return (int)ExitCode.FatalError;
        }
    }

    static async Task DiscoverMethods(TestEnvironment environment, ITestProject? testProject, IReport discoveryReport)
    {
        var runner = new Runner(environment, testProject, discoveryReport);
        await runner.Discover();
    }

    static async Task<ExitCode> Run(TestEnvironment environment, ITestProject? testProject, IReport[] reports, Func<Runner, Task<ExecutionSummary>> run)
    {
        var runner = new Runner(environment, testProject, reports);
            
        var summary = await run(runner);

        if (summary.Total == 0)
            return ExitCode.Failure;

        if (summary.Failed > 0)
            return ExitCode.Failure;

        return ExitCode.Success;
    }

    static IEnumerable<IReport> DefaultReports(TestEnvironment environment)
    {
        if (Try(AzureReport.Create, environment, out var azure))
            yield return azure;

        if (Try(TeamCityReport.Create, environment, out var teamCity))
            yield return teamCity;

        yield return ConsoleReport.Create(environment);
    }
}