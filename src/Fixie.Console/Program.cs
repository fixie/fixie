using System;
using System.IO;
using Fixie.Results;

namespace Fixie.Console
{
    using Console = System.Console;

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

                    Console.WriteLine("Usage: Fixie.Console [custom-options] assembly-path...");
                    return FatalError;
                }

                var executionResult = new ExecutionResult();

                foreach (var assemblyPath in commandLineParser.AssemblyPaths)
                {
                    var result = Execute(assemblyPath, args);

                    executionResult.Add(result);
                }

                return executionResult.Failed;
            }
            catch (Exception exception)
            {
                using (Foreground.Red)
                    Console.WriteLine("Fatal Error: {0}", exception);
                return FatalError;
            }
        }

        static AssemblyResult Execute(string assemblyPath, string[] args)
        {
            var assemblyFullPath = Path.GetFullPath(assemblyPath);

            using (var environment = new ExecutionEnvironment(assemblyFullPath))
            {
                var runner = environment.Create<ConsoleRunner>();
                return runner.RunAssembly(assemblyFullPath, args);
            }
        }
    }
}