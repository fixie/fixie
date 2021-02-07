namespace Fixie.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
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

        public static async Task<int> Main(Assembly assembly, string[] customArguments)
        {
            try
            {
                return (int) await RunAssemblyAsync(assembly, customArguments);
            }
            catch (Exception exception)
            {
                using (Foreground.Red)
                    WriteLine($"Fatal Error: {exception}");

                return (int)ExitCode.FatalError;
            }
        }

        static async Task<ExitCode> RunAssemblyAsync(Assembly assembly, string[] customArguments)
        {
            var listeners = DefaultExecutionListeners().ToArray();
            var runner = new Runner(assembly, customArguments, listeners);

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

        static IEnumerable<Listener> DefaultExecutionListeners()
        {
            if (Try(AzureListener.Create, out var azure))
                yield return azure;

            if (Try(AppVeyorListener.Create, out var appVeyor))
                yield return appVeyor;

            if (Try(XmlListener.Create, out var xml))
                yield return xml;

            if (Try(TeamCityListener.Create, out var teamCity))
                yield return teamCity;
            else
                yield return ConsoleListener.Create();
        }
    }
}