namespace Fixie.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Cli;
    using Listeners;
    using static System.Console;
    using static Maybe;

    public class EntryPoint
    {
        enum ExitCode
        {
            Success = 0,
            Failure = 1,
            FatalError = -1
        }

        public static int Main(Assembly assembly, string[] arguments)
        {
            try
            {
                CommandLine.Partition(arguments, out var runnerArguments, out var customArguments);

                var options = CommandLine.Parse<Options>(runnerArguments);

                options.Validate();

                return (int)RunAssembly(assembly, options, customArguments);
            }
            catch (Exception exception)
            {
                using (Foreground.Red)
                    WriteLine($"Fatal Error: {exception}");

                return (int)ExitCode.FatalError;
            }
        }

        static ExitCode RunAssembly(Assembly assembly, Options options, string[] customArguments)
        {
            var listeners = DefaultExecutionListeners(options).ToList();
            var bus = new Bus(listeners);
            var assemblyRunner = new AssemblyRunner(assembly, bus, customArguments);

            var summary = assemblyRunner.Run();

            if (summary.Total == 0)
                return ExitCode.FatalError;

            if (summary.Failed > 0)
                return ExitCode.Failure;

            return ExitCode.Success;
        }

        static IEnumerable<Listener> DefaultExecutionListeners(Options options)
        {
            if (Try(AzureListener.Create, out var azure))
                yield return azure;

            if (Try(AppVeyorListener.Create, out var appVeyor))
                yield return appVeyor;

            if (Try(() => ReportListener.Create(options), out var report))
                yield return report;

            if (Try(TeamCityListener.Create, out var teamCity))
                yield return teamCity;
            else
                yield return new ConsoleListener();
        }
    }
}