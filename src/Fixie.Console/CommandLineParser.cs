namespace Fixie.ConsoleRunner
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Execution;

    public class CommandLineParser
    {
        static readonly string[] SupportedReportFormats = { "NUnit", "xUnit" };

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
                errors.Add("Only one test assembly path may be specified.");
            else
                AssemblyPath = assemblyPaths.Single();

            var formats = options[CommandLineOption.ReportFormat];

            foreach (var format in formats)
                if (!SupportedReportFormats.Contains(format, StringComparer.CurrentCultureIgnoreCase))
                    errors.Add($"The specified report format, '{format}', is not supported.");

            if (formats.Count > 1)
                errors.Add("To avoid writing multiple reports over the same file, only one report format may be specified.");

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

        public IEnumerable<string> Errors { get; }

        public bool HasErrors => Errors.Any();

        static bool IsKey(string item) => item.StartsWith("--");

        static string KeyName(string item) => item.Substring("--".Length);

        public static string Usage()
        {
            return new StringBuilder()
                .AppendLine("Usage: Fixie.Console [--ReportFormat <NUnit|xUnit>] [--TeamCity <on|off>] [--<key> <value>]... assembly-path")
                .AppendLine()
                .AppendLine()
                .AppendLine("    --ReportFormat <NUnit|xUnit>")
                .AppendLine("        Write test results to a file, using NUnit or xUnit XML format.")
                .AppendLine()
                .AppendLine("    --xUnitXml <output-file>")
                .AppendLine("        Write test results to the specified file, using xUnit-style XML.")
                .AppendLine()
                .AppendLine("    --TeamCity <on|off>")
                .AppendLine("        When this option is *not* specified, the need for TeamCity-")
                .AppendLine("        formatted console output is automatically detected. Use this")
                .AppendLine("        option to force TeamCity-formatted output on or off.")
                .AppendLine()
                .AppendLine("    --<key> <value>")
                .AppendLine("        Specifies custom key/value pairs made available to custom")
                .AppendLine("        conventions. If multiple custom options are declared with the")
                .AppendLine("        same <key>, *all* of the declared <value>s will be")
                .AppendLine("        available to the convention at runtime under that <key>.")
                .AppendLine()
                .AppendLine("    assembly-path")
                .AppendLine("        A path indicating the test assembly file. Exactly one test")
                .AppendLine("        assembly must be specified.")
                .ToString();
        }

        static bool AssemblyDirectoryContainsFixie(string assemblyPath)
            => File.Exists(Path.Combine(Path.GetDirectoryName(assemblyPath), "Fixie.dll"));
    }
}