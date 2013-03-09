using System;
using System.Linq;
using System.Reflection;

namespace Fixie.Console
{
    class Program
    {
        const int FatalError = -1;

        static int Main(string[] args)
        {
            try
            {
                if (args.Length != 1)
                {
                    Line("Usage: Fixie.Console [assembly_file]");
                    return FatalError;
                }

                var assemblyFile = args.Single();
                var result = Execute(assemblyFile);

                return result.Failed;
            }
            catch (Exception ex)
            {
                Line(ex.ToString());
                return FatalError;
            }
        }

        static Result Execute(string assemblyFile)
        {
            var assembly = Assembly.LoadFrom(assemblyFile);
            var listener = new ConsoleListener();
            var runner = new Runner(listener);
            var configuration = new DefaultConfiguration(assembly);
            var result = runner.Execute(configuration);

            Line("Tests: {0}, Failures: {1}", result.Total, result.Failed);

            return result;
        }

        static void Line(string format, params object[] args)
        {
            System.Console.WriteLine(format, args);
        }
    }
}