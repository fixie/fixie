using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Fixie.Execution;
using Fixie.Reports;

namespace Fixie.ConsoleRunner
{
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

                foreach (var assemblyPath in commandLineParser.AssemblyPaths)
                {
                    if (!File.Exists(assemblyPath))
                    {
                        using (Foreground.Red)
                            Console.WriteLine("Specified test assembly does not exist: " + assemblyPath);

                        Console.WriteLine();
                        Console.WriteLine(CommandLineParser.Usage());
                        return FatalError;
                    }
                }

                var executionResult = new ExecutionResult();

                foreach (var assemblyPath in commandLineParser.AssemblyPaths)
                {
                    var result = Execute(assemblyPath, commandLineParser.Options);

                    executionResult.Add(result);
                }

                ProduceReports(commandLineParser.Options, executionResult);

                return executionResult.Failed;
            }
            catch (Exception exception)
            {
                using (Foreground.Red)
                    Console.WriteLine("Fatal Error: {0}", exception);
                return FatalError;
            }
        }

        static void ProduceReports(Options options, ExecutionResult executionResult)
        {
            if (options.Contains(CommandLineOption.NUnitXml))
            {
                var report = new NUnitXmlReport();

                var xDocument = report.Transform(executionResult);

                foreach (var fileName in options[CommandLineOption.NUnitXml])
                    xDocument.Save(fileName, SaveOptions.None);
            }

            if (options.Contains(CommandLineOption.XUnitXml))
            {
                var report = new XUnitXmlReport();

                var xDocument = report.Transform(executionResult);

                foreach (var fileName in options[CommandLineOption.XUnitXml])
                    xDocument.Save(fileName, SaveOptions.None);
            }
        }

        static AssemblyResult Execute(string assemblyPath, Options options)
        {
            if (ShouldUseTeamCityListener(options))
            {
                using (var listener = new TeamCityListener())
                    return Execute(assemblyPath, options, listener);
            }

            if (ShouldUseAppVeyorListener())
            {
                using (var listener = new AppVeyorListener())
                    return Execute(assemblyPath, options, listener);
            }

            using (var listener = new ConsoleListener())
                return Execute(assemblyPath, options, listener);
        }

        static AssemblyResult Execute(string assemblyPath, Options options, Listener listener)
        {
            using (var environment = new ExecutionEnvironment(assemblyPath))
                return environment.RunAssembly(options, listener);
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