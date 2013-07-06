using System;
using System.Linq;

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
                var assemblyPaths = new CommandLineParser(args).AssemblyPaths.ToArray();

                if (assemblyPaths.Length == 0)
                {
                    Console.WriteLine("Usage: Fixie.Console [custom-options] assembly_file...");
                    return FatalError;
                }

                int failed = 0;

                foreach (var assemblyPath in assemblyPaths)
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