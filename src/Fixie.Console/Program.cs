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
            catch (Exception exception)
            {
                using (Foreground.Red)
                    WriteLine($"Fatal Error: {exception}");

                return FatalError;
            }
        }
    }
}