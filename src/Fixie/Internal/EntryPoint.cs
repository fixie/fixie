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
            var rootDirectory = Directory.GetCurrentDirectory();
            var context = new TestContext(assembly, console, rootDirectory, customArguments);

            using var boundary = new ConsoleRedirectionBoundary();

            try
            {
                return (int) await RunAssembly(context);
            }
            catch (Exception exception)
            {
                using (Foreground.Red)
                    console.WriteLine($"Fatal Error: {exception}");

                return (int)ExitCode.FatalError;
            }
        }

        static async Task<ExitCode> RunAssembly(TestContext context)
        {
            var reports = DefaultReports(context).ToArray();
            var runner = new Runner(context, reports);

            var pattern = GetEnvironmentVariable("FIXIE:TESTS");

            var summary = pattern == null
                ? await runner.Run()
                : await runner.Run(new TestPattern(pattern));

            if (summary.Total == 0)
                return ExitCode.FatalError;

            if (summary.Failed > 0)
                return ExitCode.Failure;

            return ExitCode.Success;
        }

        static IEnumerable<IReport> DefaultReports(TestContext context)
        {
            TextWriter console = context.Console;

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