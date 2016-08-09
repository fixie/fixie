namespace Fixie.Runner
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public class CommandLineParser
    {
        public CommandLineParser(params string[] args)
        {
            var queue = new Queue<string>(args);

            var assemblyPaths = new List<string>();
            var options = new Options();
            var errors = new List<string>();

            while (queue.Any())
            {
                var item = queue.Dequeue();

                if (IsKey(item))
                {
                    if (!queue.Any() || IsKey(queue.Peek()))
                    {
                        errors.Add($"Option {item} is missing its required value.");
                        break;
                    }

                    var key = KeyName(item);
                    var value = queue.Dequeue();

                    options.Add(key, value);
                }
                else
                {
                    assemblyPaths.Add(item);
                }
            }

            if (assemblyPaths.Count == 0)
                errors.Add("Missing required test assembly path.");
            else if (assemblyPaths.Count > 1)
                errors.Add("Specify a single test assembly path. To test multiple assemblies, invoke the test runner once per test assembly.");
            else
                AssemblyPath = assemblyPaths.Single();

            var formats = options[CommandLineOption.ReportFormat];

            foreach (var format in formats)
            {
                ReportFormat parsed;
                if (!Enum.TryParse(format, true, out parsed))
                    errors.Add($"The specified report format, '{format}', is not supported.");
            }

            if (!errors.Any())
            {
                if (!File.Exists(AssemblyPath))
                    errors.Add("Specified test assembly does not exist: " + AssemblyPath);
                else if (!AssemblyDirectoryContainsFixie(AssemblyPath))
                    errors.Add($"Specified assembly {AssemblyPath} does not appear to be a test assembly. Ensure that it references Fixie.dll and try again.");
            }

            Options = options;
            Errors = errors.ToArray();
        }

        public string AssemblyPath { get; }

        public Options Options { get; }

        public IReadOnlyCollection<string> Errors { get; }

        public bool HasErrors => Errors.Any();

        static bool IsKey(string item) => item.StartsWith("--");

        static string KeyName(string item) => item.Substring("--".Length);

        static bool AssemblyDirectoryContainsFixie(string assemblyPath)
            => File.Exists(Path.Combine(Path.GetDirectoryName(assemblyPath), "Fixie.dll"));
    }
}