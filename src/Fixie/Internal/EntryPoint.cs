namespace Fixie.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Listeners;
    using static System.Console;
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

        public static int Main(Assembly assembly, string[] customArguments)
        {
            try
            {
                return (int)RunAssembly(assembly, customArguments);
            }
            catch (Exception exception)
            {
                using (Foreground.Red)
                    WriteLine($"Fatal Error: {exception}");

                return (int)ExitCode.FatalError;
            }
        }

        static ExitCode RunAssembly(Assembly assembly, string[] customArguments)
        {
            var listeners = DefaultExecutionListeners().ToArray();
            var runner = new Runner(assembly, customArguments, listeners);

            var testsPattern = GetEnvironmentVariable("FIXIE:TESTS");

            var summary = testsPattern == null
                ? runner.Run()
                : runner.Run(testsPattern);

            if (summary.Total == 0)
                return ExitCode.FatalError;

            if (summary.Failed > 0)
                return ExitCode.Failure;

            return ExitCode.Success;
        }

        static IEnumerable<Listener> DefaultExecutionListeners()
        {
            if (Try(AzureListener.Create, out var azure))
                yield return azure;

            if (Try(AppVeyorListener.Create, out var appVeyor))
                yield return appVeyor;

            if (Try(ReportListener.Create, out var report))
                yield return report;

            if (Try(TeamCityListener.Create, out var teamCity))
                yield return teamCity;
            else
                yield return ConsoleListener.Create();
        }
    }
}