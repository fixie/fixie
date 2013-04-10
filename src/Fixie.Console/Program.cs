using System;
using System.Linq;
using System.Reflection;

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

                var assemblyFile = args.Single();
                var result = Execute(assemblyFile);

                return result.Failed;
            }
            catch (Exception ex)
            {
                using (Foreground.Red)
                    Console.WriteLine("Fatal Error: {0}", ex);
                return FatalError;
            }
        }

        static Result Execute(string assemblyFile)
        {
            var assembly = Assembly.LoadFrom(assemblyFile);
            var listener = new ConsoleListener();
            var convention = new DefaultConvention();
            var suite = new Suite(convention, assembly.GetTypes());

            using (WorkingDirectory.LocationOf(assembly))
            {
                var result = suite.Execute(listener);

                Console.WriteLine("{0} total, {1} failed", result.Total, result.Failed);

                return result;
            }
        }
    }
}