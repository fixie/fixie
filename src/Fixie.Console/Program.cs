using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Fixie.Reports;
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

                ProduceReports(commandLineParser.Options, executionResult);

                return executionResult.Failed;
            }
            catch (Exception exception)
            {
                using (Foreground.Red)
                    Console.WriteLine("Fatal Error: {0}", exception);
                return FatalError;
            }
        }

        static void ProduceReports(ILookup<string, string> options, ExecutionResult executionResult)
        {
            if (options.Contains(CommandLineOption.NUnitXml))
            {
                var report = new NUnitXmlReport();

                var xDocument = report.Transform(executionResult);

                foreach (var fileName in options[CommandLineOption.NUnitXml])
                    xDocument.Save(fileName, SaveOptions.None);
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