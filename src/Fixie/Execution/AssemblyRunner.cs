namespace Fixie.Execution
{
    using System;
    using System.Reflection;
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

                var assemblyFullPath = Assembly.GetEntryAssembly().Location;

                using (var environment = new ConsoleExecutionEnvironment(assemblyFullPath))
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