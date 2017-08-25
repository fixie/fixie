namespace Fixie.Execution
{
    using System;
    using System.IO;
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

                var assemblyDirectory = Path.GetDirectoryName(assemblyFullPath);

                using (var executionProxy = new ExecutionProxy(assemblyDirectory))
                    return executionProxy.RunAssembly(assemblyFullPath, arguments);
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