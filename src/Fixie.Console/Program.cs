namespace Fixie.Console
{
    using System;
    using Cli;
    using Execution;
    using static System.Console;

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
                    WriteLine(exception.Message);

                Help();

                return FatalError;
            }
            catch (Exception exception)
            {
                using (Foreground.Red)
                    WriteLine($"Fatal Error: {exception}");

                return FatalError;
            }
        }

        static void Help()
        {
            WriteLine();
            WriteLine("Usage: Fixie.Console.exe assembly-path [patterns]... [--report path] [--team-city <on|off>] [convention arguments]...");
            WriteLine();
            WriteLine();
            WriteLine("    assembly-path");
            WriteLine("        A path indicating the test assembly file. Exactly one test");
            WriteLine("        assembly must be specified.");
            WriteLine();
            WriteLine("    patterns");
            WriteLine("        Zero or more test name patterns. When provided, a test");
            WriteLine("        will run only if it matches at least one of the patterns.");
            WriteLine();
            WriteLine("    --report path");
            WriteLine("        Write test results to the specified path, using the xUnit XML format.");
            WriteLine();
            WriteLine("    --team-city <on|off>");
            WriteLine("        When this option is omitted, the runner detects the need for");
            WriteLine("        TeamCity-formatted console output. Use this option to force");
            WriteLine("        TeamCity output on or off.");
            WriteLine();
            WriteLine("    convention arguments");
            WriteLine("        Arbitrary arguments made available to conventions at runtime.");
            WriteLine();
        }
    }
}