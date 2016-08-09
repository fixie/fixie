namespace Fixie.Runner
{
    using System;
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
                Options options;
                try
                {
                    options = CommandLine.Parse<Options>(args);
                    options.Validate();
                }
                catch (CommandLineException exception)
                {
                    using (Foreground.Red)
                        Console.WriteLine(exception.Message);

                    Console.WriteLine();
                    Console.WriteLine(Usage());
                    return FatalError;
                }

                return RunAssembly(options, args);
            }
            catch (Exception exception)
            {
                using (Foreground.Red)
                    Console.WriteLine($"Fatal Error: {exception}");
                return FatalError;
            }
        }

        static int RunAssembly(Options options, string[] args)
        {
            using (var environment = new ExecutionEnvironment(options.AssemblyPath))
            {
                if (ShouldUseTeamCityListener(options))
                    environment.Subscribe<TeamCityListener>();
                else
                    environment.Subscribe<ConsoleListener>();

                if (ShouldUseAppVeyorListener())
                    environment.Subscribe<AppVeyorListener>();

                if (options.ReportFormat == ReportFormat.NUnit)
                    environment.Subscribe<ReportListener<NUnitXml>>();
                else if (options.ReportFormat == ReportFormat.xUnit)
                    environment.Subscribe<ReportListener<XUnitXml>>();

                return environment.RunAssembly(args);
            }
        }

        static bool ShouldUseTeamCityListener(Options options)
        {
            var teamCityExplicitlyEnabled = options.TeamCity == true;

            var runningUnderTeamCity = Environment.GetEnvironmentVariable("TEAMCITY_PROJECT_NAME") != null;

            return teamCityExplicitlyEnabled || runningUnderTeamCity;
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