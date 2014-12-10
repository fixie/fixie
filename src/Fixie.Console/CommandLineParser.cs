using System;
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
            var optionList = new Lookup();
            var errors = new List<string>();

            while (queue.Any())
            {
                var item = queue.Dequeue();

                if (IsKey(item))
                {
                    var key = KeyName(item);

                    key = CorrectlyCasedKey(key);

                    if (key == null)
                    {
                        errors.Add(string.Format("Option {0} is not recognized.", item));
                        break;
                    }

                    if (!queue.Any() || IsKey(queue.Peek()))
                    {
                        errors.Add(string.Format("Option {0} is missing its required value.", item));
                        break;
                    }

                    var value = queue.Dequeue();

                    if (key == CommandLineOption.Parameter)
                    {
                        if (value.Contains("="))
                        {
                            var equalSignIndex = value.IndexOf('=');

                            if (equalSignIndex == 0)
                            {
                                errors.Add(string.Format("Custom parameter {0} is missing its required key.", value));
                                break;
                            }

                            key = value.Substring(0, equalSignIndex);
                            value = value.Substring(equalSignIndex + 1);
                        }
                        else
                        {
                            key = value;
                            value = "on";
                        }
                    }

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

        public Lookup Options { get; private set; }

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

        static string CorrectlyCasedKey(string key)
        {
            foreach (var option in CommandLineOption.GetAll())
                if (String.Equals(key, option, StringComparison.OrdinalIgnoreCase))
                    return option;

            return null;
        }

        public static string Usage()
        {
            return new StringBuilder()
                .AppendLine("Usage: Fixie.Console [--NUnitXml <output-file>] [--XUnitXml <output-file>] [--TeamCity <on|off>] [--parameter <name>=<value>] assembly-path...")
                .AppendLine()
                .AppendLine()
                .AppendLine("    --NUnitXml <output-file>")
                .AppendLine("        Write test results to the specified file, using NUnit-style XML.")
                .AppendLine()
                .AppendLine("    --XUnitXml <output-file>")
                .AppendLine("        Write test results to the specified file, using xUnit-style XML.")
                .AppendLine()
                .AppendLine("    --TeamCity <on|off>")
                .AppendLine("        When this option is *not* specified, the need for TeamCity-")
                .AppendLine("        formatted console output is automatically detected. Use this")
                .AppendLine("        option to force TeamCity-formatted output on or off.")
                .AppendLine()
                .AppendLine("    --parameter <name>=<value>")
                .AppendLine("        Specifies any number of arbitrary name/value pairs, made available")
                .AppendLine("        to custom conventions. If multiple --parameter options are declared")
                .AppendLine("        with the same <name>, *all* of the declared <value>s will be")
                .AppendLine("        available at runtime.")
                .AppendLine()
                .AppendLine("    assembly-path...")
                .AppendLine("        One or more paths indicating test assembly files.  At least one")
                .AppendLine("        test assembly must be specified.")
                .ToString();
        }
    }
}