using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Fixie.ConsoleRunner
{
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

            if (!errors.Any())
            {
                if (assemblyPaths.Count == 0)
                    errors.Add("Missing required test assembly path.");
                else if (assemblyPaths.Count > 1)
                    errors.Add("Only one test assembly path is allowed. Invoke the test runner once per test assembly.");
                else
                {
                    AssemblyPath = assemblyPaths.Single();

                    if (!File.Exists(assemblyPaths.Single()))
                        errors.Add("Specified test assembly does not exist: " + AssemblyPath);
                }
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
                .AppendLine("Usage: Fixie.Console assembly-path [--NUnitXml <output-file>] [--xUnitXml <output-file>] [--TeamCity <on|off>] [--<key> <value>]...")
                .AppendLine()
                .AppendLine("    assembly-path")
                .AppendLine("        A path to a test assembly containing the tests to execute.")
                .AppendLine()
                .AppendLine("    --NUnitXml <output-file>")
                .AppendLine("        Write test results to the specified file, using NUnit-style XML.")
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
                .ToString();
        }
    }
}