namespace Fixie.Runner
{
    using System;
    using System.Linq;
    using System.Text;
    using Cli;
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
                    Console.WriteLine(Usage());
                    return FatalError;
                }

                return RunAssembly(commandLineParser, args);
            }
            catch (Exception exception)
            {
                using (Foreground.Red)
                    Console.WriteLine($"Fatal Error: {exception}");
                return FatalError;
            }
        }

        static int RunAssembly(CommandLineParser commandLineParser, string[] args)
        {
            var options = commandLineParser.Options;

            using (var environment = new ExecutionEnvironment(commandLineParser.AssemblyPath))
            {
                if (ShouldUseTeamCityListener(options))
                    environment.Subscribe<TeamCityListener>();
                else
                    environment.Subscribe<ConsoleListener>();

                if (ShouldUseAppVeyorListener())
                    environment.Subscribe<AppVeyorListener>();

                foreach (var format in options[CommandLineOption.ReportFormat])
                {
                    if (String.Equals(format, "NUnit", StringComparison.CurrentCultureIgnoreCase))
                        environment.Subscribe<ReportListener<NUnitXml>>();

                    else if (String.Equals(format, "xUnit", StringComparison.CurrentCultureIgnoreCase))
                        environment.Subscribe<ReportListener<XUnitXml>>();
                }

                return environment.RunAssembly(args);
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

        static string Usage()
        {
            return new StringBuilder()
                .AppendLine("Usage: dotnet-test-fixie.exe assembly-path [--ReportFormat <NUnit|xUnit>] [--TeamCity <on|off>] [--<key> <value>]...")
                .AppendLine()
                .AppendLine()
                .AppendLine("    assembly-path")
                .AppendLine("        A path indicating the test assembly the run.")
                .AppendLine()
                .AppendLine("    --ReportFormat <NUnit|xUnit>")
                .AppendLine("        Write test results to a file, using NUnit or xUnit XML format.")
                .AppendLine()
                .AppendLine("    --TeamCity <on|off>")
                .AppendLine("        When this option is *not* specified, the need for TeamCity-")
                .AppendLine("        formatted console output is automatically detected. Use this")
                .AppendLine("        option to force TeamCity-formatted output on or off.")
                .AppendLine()
                .AppendLine("    --<key> <value>")
                .AppendLine("        Specifies custom key/value pairs made available to custom")
                .AppendLine("        conventions. If multiple custom options are declared with the")
                .AppendLine("        same <key>, *all* of the declared <value>s will be")
                .AppendLine("        available to the convention at runtime under that <key>.")
                .ToString();
        }
    }
}