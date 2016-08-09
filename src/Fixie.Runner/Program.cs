namespace Fixie.Runner
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Cli;
    using Execution;
    using Reports;

    class Program
    {
        const int FatalError = -1;

        [STAThread]
        static int Main(string[] arguments)
        {
            var fixieArguments = new List<string>();
            var conventionArguments = new List<string>();

            bool separatorFound = false;
            foreach (var arg in arguments)
            {
                if (arg == "--")
                {
                    separatorFound = true;
                    continue;
                }

                if (separatorFound)
                    conventionArguments.Add(arg);
                else
                    fixieArguments.Add(arg);
            }

            try
            {
                Options options;
                try
                {
                    string[] unusedArguments;
                    options = CommandLine.Parse<Options>(fixieArguments, out unusedArguments);

                    using (Foreground.Yellow)
                        foreach (var unusedArgument in unusedArguments)
                            Console.WriteLine($"The argument '{unusedArgument}' was unexpected and will be ignored.");

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

                return RunAssembly(options, conventionArguments);
            }
            catch (Exception exception)
            {
                using (Foreground.Red)
                    Console.WriteLine($"Fatal Error: {exception}");
                return FatalError;
            }
        }

        static int RunAssembly(Options options, IReadOnlyList<string> conventionArguments)
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

                return environment.RunAssembly(conventionArguments.ToArray());
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
                .AppendLine("Usage: dotnet test [dotnet arguments]")
                .AppendLine("       dotnet test [dotnet arguments] -- [--report-format <NUnit|xUnit>] [--team-city <on|off>] [--] [convention arguments]...")
                .AppendLine()
                .AppendLine()
                .AppendLine("    dotnet arguments")
                .AppendLine("        Arguments to be used by 'dotnet test'. See 'dotnet test --help'.")
                .AppendLine()
                .AppendLine("    --report-format <NUnit|xUnit>")
                .AppendLine("        Write test results to a file, using NUnit or xUnit XML format.")
                .AppendLine()
                .AppendLine("    --team-city <on|off>")
                .AppendLine("        When this option is *not* specified, the need for TeamCity-")
                .AppendLine("        formatted console output is automatically detected. Use this")
                .AppendLine("        option to force TeamCity-formatted output on or off.")
                .AppendLine()
                .AppendLine("    --")
                .AppendLine("        When present, all of the following arguments will be passed along")
                .AppendLine("        for use from within a convention.")
                .AppendLine()
                .AppendLine("    convention arguments")
                .AppendLine("        Arbitrary arguments made available to conventions at runtime.")
                .ToString();
        }
    }
}