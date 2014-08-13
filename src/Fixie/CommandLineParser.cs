using System;
using System.Collections.Generic;
using System.Linq;

namespace Fixie
{
    public class CommandLineParser
    {
        public CommandLineParser(params string[] args)
        {
            var queue = new Queue<string>(args);

            var assemblyPaths = new List<string>();
            var optionList = new List<KeyValuePair<string, string>>();
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

                    key = CorrectlyCasedKey(key);

                    if (key == null)
                    {
                        errors.Add(string.Format("Option {0} is not recognized.", item));
                        break;
                    }

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

                    optionList.Add(new KeyValuePair<string, string>(key, value));
                }
                else
                {
                    assemblyPaths.Add(item);
                }
            }

            if (!errors.Any() && !assemblyPaths.Any())
                errors.Add("Missing required test assembly path(s).");

            AssemblyPaths = assemblyPaths.ToArray();
            Options = optionList.ToLookup(x => x.Key, x => x.Value);
            Errors = errors.ToArray();
        }

        public IEnumerable<string> AssemblyPaths { get; private set; }

        public ILookup<string, string> Options { get; private set; }

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
    }
}