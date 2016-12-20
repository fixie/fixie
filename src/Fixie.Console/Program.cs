namespace Fixie.ConsoleRunner
{
    using System;
    using System.Diagnostics;
    using System.Text;
    using System.Linq;
    using System.Xml.Linq;
    using Execution;
    using Reports;

    class Program
    {
        const int FatalError = -1;

        [STAThread]
        static int Main(string[] args)
        {
            try
            {
                var commandLineParser = new CommandLineParser(args);

                if (commandLineParser.HasErrors)
                {
                    using (Foreground.Red)
                        foreach (var error in commandLineParser.Errors)
                            Console.WriteLine(error);

                    Console.WriteLine();
                    Console.WriteLine(CommandLineParser.Usage());
                    return FatalError;
                }

                var summary = Execute(commandLineParser);

                return summary.Failed;
            }
            catch (Exception exception)
            {
                using (Foreground.Red)
                    Console.WriteLine($"Fatal Error: {exception}");
                return FatalError;
            }
        }

        static ExecutionResult Execute(CommandLineParser commandLineParser)
        {
            var options = commandLineParser.Options;

            var summary = new ExecutionResult();

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            foreach (var assemblyPath in commandLineParser.AssemblyPaths)
            {
                var result = Execute(assemblyPath, options);

                summary.Add(result);
            }

            stopwatch.Stop();

            if (summary.AssemblyResults.Count > 1)
                Summarize(summary, stopwatch.Elapsed);

            SaveReport(options, summary);

            return summary;
        }

        static void Summarize(ExecutionResult executionResult, TimeSpan elapsed)
        {
            var line = new StringBuilder();

            line.AppendFormat("{0} passed", executionResult.Passed);
            line.AppendFormat(", {0} failed", executionResult.Failed);

            if (executionResult.Skipped > 0)
                line.AppendFormat(", {0} skipped", executionResult.Skipped);

            line.AppendFormat(", took {0:N2} seconds", elapsed.TotalSeconds);

            Console.WriteLine("====== " + line + " ======");
        }

        static void SaveReport(Options options, ExecutionResult executionResult)
        {
            if (options.Contains(CommandLineOption.NUnitXml))
            {
                var xDocument = new NUnitXmlReport().Transform(executionResult);

                foreach (var fileName in options[CommandLineOption.NUnitXml])
                    xDocument.Save(fileName, SaveOptions.None);
            }

            if (options.Contains(CommandLineOption.XUnitXml))
            {
                var xDocument = new XUnitXmlReport().Transform(executionResult);

                foreach (var fileName in options[CommandLineOption.XUnitXml])
                    xDocument.Save(fileName, SaveOptions.None);
            }
        }

        static AssemblyResult Execute(string assemblyPath, Options options)
        {
            using (var environment = new ExecutionEnvironment(assemblyPath))
            {
                if (ShouldUseTeamCityListener(options))
                    using (var listener = new TeamCityListener())
                        return environment.RunAssembly(options, listener);

                if (ShouldUseAppVeyorListener())
                    using (var listener = new AppVeyorListener())
                        return environment.RunAssembly(options, listener);

                using (var listener = new ConsoleListener())
                    return environment.RunAssembly(options, listener);
            }
        }

        static bool ShouldUseTeamCityListener(Options options)
        {
            var teamCityExplicitlySpecified = options.Contains(CommandLineOption.TeamCity);

            var runningUnderTeamCity = Environment.GetEnvironmentVariable("TEAMCITY_PROJECT_NAME") != null;

            var useTeamCityListener =
                (teamCityExplicitlySpecified && options[CommandLineOption.TeamCity].First() == "on") ||
                (!teamCityExplicitlySpecified && runningUnderTeamCity);

            return useTeamCityListener;
        }

        static bool ShouldUseAppVeyorListener()
        {
            return Environment.GetEnvironmentVariable("APPVEYOR") == "True";
        }
    }
}