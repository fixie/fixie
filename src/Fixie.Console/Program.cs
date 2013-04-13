using System;
using System.Linq;

namespace Fixie.Console
{
    using Console = System.Console;

    class Program
    {
        const int FatalError = -1;

        static int Main(string[] args)
        {
            try
            {
                if (args.Length != 1)
                {
                    Console.WriteLine("Usage: Fixie.Console [assembly_file]");
                    return FatalError;
                }

                var assemblyPath = args.Single();
                var result = Execute(assemblyPath);

                return result.Failed;
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