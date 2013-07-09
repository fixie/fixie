using System;

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

                int failed = 0;

                foreach (var assemblyPath in commandLineParser.AssemblyPaths)
                {
                    var result = Execute(assemblyPath, args);

                    failed += result.Failed;
                }

                return failed;
            }
            catch (Exception ex)
            {
                using (Foreground.Red)
                    Console.WriteLine("Fatal Error: {0}", ex);
                return FatalError;
            }
        }

        static Result Execute(string assemblyPath, string[] args)
        {
            using (var environment = new ExecutionEnvironment(assemblyPath))
            {
                var runner = environment.Create<ConsoleRunner>();
                return runner.RunAssembly(assemblyPath, args);
            }
        }
    }
}