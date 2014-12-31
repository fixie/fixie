using System.Collections.Generic;
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
            var optionList = new Options();
            var errors = new List<string>();

            while (queue.Any())
            {
                var item = queue.Dequeue();

                if (IsKey(item))
                {
                    if (!queue.Any() || IsKey(queue.Peek()))
                    {
                        errors.Add(string.Format("Option {0} is missing its required value.", item));
                        break;
                    }

                    var key = KeyName(item);
                    var value = queue.Dequeue();

                    optionList.Add(key, value);
                }
                else
                {
                    assemblyPaths.Add(item);
                }
            }

            if (!errors.Any() && !assemblyPaths.Any())
                errors.Add("Missing required test assembly path(s).");

            AssemblyPaths = assemblyPaths.ToArray();
            Options = optionList;
            Errors = errors.ToArray();
        }

        public IEnumerable<string> AssemblyPaths { get; private set; }

        public Options Options { get; private set; }

        public IEnumerable<string> Errors { get; private set; }

        public bool HasErrors
        {
            get { return Errors.Any(); }
        }

        static bool IsKey(string item)
        {
            return item.StartsWith("--");
        }

        static string KeyName(string item)
        {
            return item.Substring("--".Length);
        }

        public static string Usage()
        {
            return new StringBuilder()
                .AppendLine("Usage: Fixie.Console [--NUnitXml <output-file>] [--xUnitXml <output-file>] [--TeamCity <on|off>] [--<key> <value>]... assembly-path...")
                .AppendLine()
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
                .AppendLine()
                .AppendLine("    assembly-path...")
                .AppendLine("        One or more paths indicating test assembly files.  At least one")
                .AppendLine("        test assembly must be specified.")
                .ToString();
        }
    }
}