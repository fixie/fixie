namespace Fixie.Console
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Cli;
    using Execution;

    class Program
    {
        const int FatalError = -1;

        [STAThread]
        static int Main(string[] arguments)
        {
            try
            {
                SplitArguments(arguments, out string[] runnerArguments, out string[] conventionArguments);

                var options = ParseRunnerArguments(runnerArguments);

                using (var environment = new ExecutionEnvironment(options.AssemblyPath))
                    return environment.RunAssembly(runnerArguments, conventionArguments);
            }
            catch (CommandLineException exception)
            {
                using (Foreground.Red)
                    Console.WriteLine(exception.Message);

                Console.WriteLine();
                Console.WriteLine(Usage());
                return FatalError;
            }
            catch (Exception exception)
            {
                using (Foreground.Red)
                    Console.WriteLine($"Fatal Error: {exception}");
                return FatalError;
            }
        }

        static void SplitArguments(string[] arguments, out string[] runnerArguments, out string[] conventionArguments)
        {
            var runnerArgumentList = new List<string>();
            var conventionArgumentList = new List<string>();

            bool separatorFound = false;
            foreach (var arg in arguments)
            {
                if (arg == "--")
                {
                    separatorFound = true;
                    continue;
                }

                if (separatorFound)
                    conventionArgumentList.Add(arg);
                else
                    runnerArgumentList.Add(arg);
            }

            runnerArguments = runnerArgumentList.ToArray();
            conventionArguments = conventionArgumentList.ToArray();
        }

        static Options ParseRunnerArguments(string[] runnerArguments)
        {
            var options = CommandLine.Parse<Options>(runnerArguments, out string[] unusedArguments);

            using (Foreground.Yellow)
                foreach (var unusedArgument in unusedArguments)
                    Console.WriteLine($"The argument '{unusedArgument}' was unexpected and will be ignored.");

            options.Validate();
            return options;
        }

        static string Usage()
        {
            return new StringBuilder()
                .AppendLine("Usage: Fixie.Console.exe assembly-path [--report path] [--team-city <on|off>] [--] [convention arguments]...")
                .AppendLine()
                .AppendLine()
                .AppendLine("    assembly-path")
                .AppendLine("        A path indicating the test assembly file. Exactly one test")
                .AppendLine("        assembly must be specified.")
                .AppendLine()
                .AppendLine("    --report path")
                .AppendLine("        Write test results to the specified path, using the xUnit XML format.")
                .AppendLine()
                .AppendLine("    --team-city <on|off>")
                .AppendLine("        When this option is omitted, the runner detects the need for")
                .AppendLine("        TeamCity-formatted console output. Use this option to force")
                .AppendLine("        TeamCity output on or off.")
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