namespace Fixie.Internal
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Reports;
    using static System.Environment;
    using static Maybe;

    public class EntryPoint
    {
        enum ExitCode
        {
            Success = 0,
            Failure = 1,
            FatalError = -1
        }

        public static async Task<int> Main(Assembly assembly, string[] customArguments)
        {
            var console = Console.Out;
            var rootPath = Directory.GetCurrentDirectory();
            var environment = new TestEnvironment(assembly, console, rootPath, customArguments);

            using var boundary = new ConsoleRedirectionBoundary();

            try
            {
                return (int) await RunAssembly(environment);
            }
            catch (Exception exception)
            {
                using (Foreground.Red)
                    console.WriteLine($"Fatal Error: {exception}");

                return (int)ExitCode.FatalError;
            }
        }

        static async Task<ExitCode> RunAssembly(TestEnvironment environment)
        {
            var reports = DefaultReports(environment).ToArray();
            var runner = new Runner(environment, reports);

            var pattern = GetEnvironmentVariable("FIXIE:TESTS_PATTERN");

            var summary = pattern == null
                ? await runner.Run()
                : await runner.Run(new TestPattern(pattern));

            if (summary.Total == 0)
                return ExitCode.FatalError;

            if (summary.Failed > 0)
                return ExitCode.Failure;

            return ExitCode.Success;
        }

        static IEnumerable<IReport> DefaultReports(TestEnvironment environment)
        {
            TextWriter console = environment.Console;

            if (Try(() => AzureReport.Create(console), out var azure))
                yield return azure;

            if (Try(AppVeyorReport.Create, out var appVeyor))
                yield return appVeyor;

            if (Try(() => TeamCityReport.Create(console), out var teamCity))
                yield return teamCity;

            yield return ConsoleReport.Create(console);
        }
    }
}