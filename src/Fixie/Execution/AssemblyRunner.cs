namespace Fixie.Execution
{
    using System;
    using Cli;

    public class AssemblyRunner
    {
        const int FatalError = -1;

        public static int Main(string[] arguments)
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
                    Console.WriteLine($"Fatal Error: {exception}");

                return FatalError;
            }
        }
    }
}