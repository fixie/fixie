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

            try
            {
                return (int) await RunAssemblyAsync(assembly, console, customArguments);
            }
            catch (Exception exception)
            {
                using (Foreground.Red)
                    console.WriteLine($"Fatal Error: {exception}");

                return (int)ExitCode.FatalError;
            }
        }

        static async Task<ExitCode> RunAssemblyAsync(Assembly assembly, TextWriter console, string[] customArguments)
        {
            var reports = DefaultReports(console).ToArray();
            var runner = new Runner(assembly, console, customArguments, reports);

            var pattern = GetEnvironmentVariable("FIXIE:TESTS");

            var summary = pattern == null
                ? await runner.RunAsync()
                : await runner.RunAsync(new TestPattern(pattern));

            if (summary.Total == 0)
                return ExitCode.FatalError;

            if (summary.Failed > 0)
                return ExitCode.Failure;

            return ExitCode.Success;
        }

        static IEnumerable<Report> DefaultReports(TextWriter console)
        {
            if (Try(() => AzureReport.Create(console), out var azure))
                yield return azure;

            if (Try(AppVeyorReport.Create, out var appVeyor))
                yield return appVeyor;

            if (Try(XmlReport.Create, out var xml))
                yield return xml;

            if (Try(() => TeamCityReport.Create(console), out var teamCity))
                yield return teamCity;

            yield return ConsoleReport.Create(console);
        }
    }
}