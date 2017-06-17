namespace Fixie.Console
{
    using System;
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
                var options = CommandLine.Parse<Options>(arguments);
                options.Validate();

                using (var environment = new ExecutionEnvironment(options.AssemblyPath))
                    return environment.RunAssembly(arguments);
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

        static string Usage()
        {
            return new StringBuilder()
                .AppendLine("Usage: Fixie.Console.exe assembly-path [--report path] [--team-city <on|off>] [convention arguments]...")
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
                .AppendLine("    convention arguments")
                .AppendLine("        Arbitrary arguments made available to conventions at runtime.")
                .ToString();
        }
    }
}