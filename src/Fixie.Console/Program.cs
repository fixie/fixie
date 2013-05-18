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
                if (args.Length == 0)
                {
                    Console.WriteLine("Usage: Fixie.Console assembly_file...");
                    return FatalError;
                }

                int failed = 0;

                foreach (var assemblyPath in args)
                {
                    var result = Execute(assemblyPath);

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

        static Result Execute(string assemblyPath)
        {
            using (var environment = new ExecutionEnvironment(assemblyPath))
            {
                var runner = environment.Create<ConsoleRunner>();
                return runner.RunAssembly(assemblyPath);
            }
        }
    }
}