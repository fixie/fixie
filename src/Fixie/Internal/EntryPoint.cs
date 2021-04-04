namespace Fixie.Internal
{
    using System;
    using System.Collections.Generic;
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
            try
            {
                return (int) await RunAssemblyAsync(assembly, customArguments);
            }
            catch (Exception exception)
            {
                using (Foreground.Red)
                    Console.WriteLine($"Fatal Error: {exception}");

                return (int)ExitCode.FatalError;
            }
        }

        static async Task<ExitCode> RunAssemblyAsync(Assembly assembly, string[] customArguments)
        {
            var reports = DefaultReports().ToArray();
            var runner = new Runner(assembly, customArguments, reports);

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

        static IEnumerable<Report> DefaultReports()
        {
            var originalStandardOut = Console.Out;

            if (Try(() => AzureReport.Create(originalStandardOut), out var azure))
                yield return azure;

            if (Try(AppVeyorReport.Create, out var appVeyor))
                yield return appVeyor;

            if (Try(XmlReport.Create, out var xml))
                yield return xml;

            if (Try(() => TeamCityReport.Create(originalStandardOut), out var teamCity))
                yield return teamCity;

            yield return ConsoleReport.Create(originalStandardOut);
        }
    }
}