using System;
using System.Collections.Generic;
using System.Linq;

namespace Fixie
{
    public class CommandLineParser
    {
        readonly IDictionary<string, List<string>> options;

        public CommandLineParser(params string[] args)
        {
            var queue = new Queue<string>(args);

            var assemblyPaths = new List<string>();
            options = new Dictionary<string, List<string>>();

            while (queue.Any())
            {
                var item = queue.Dequeue();

                if (IsKey(item))
                {
                    if (!queue.Any() || IsKey(queue.Peek()))
                        throw new Exception(string.Format("Option {0} is missing its required value.", item));

                    var key = KeyName(item);
                    var value = queue.Dequeue();

                    if (!options.ContainsKey(key))
                        options.Add(key, new List<string>());

                    options[key].Add(value);
                }
                else
                {
                    assemblyPaths.Add(item);
                }
            }

            AssemblyPaths = assemblyPaths.ToArray();
        }

        public IEnumerable<string> AssemblyPaths { get; private set; }

        public IEnumerable<string> Keys
        {
            get { return options.Keys; }
        }

        public string this[string key]
        {
            get
            {
                var values = options[key];

                if (values.Count > 1)
                    throw new ArgumentException(string.Format(
                        "Option --{0} has multiple values. Instead of using the indexer " +
                        "property, call GetAll(string) to retrieve all the values.", key));

                return values.Single();
            }
        }

        public IEnumerable<string> GetAll(string key)
        {
            return options[key].ToArray();
        }

        static bool IsKey(string item)
        {
            return item.StartsWith("--");
        }

        static string KeyName(string item)
        {
            return item.Substring("--".Length);
        }
    }
}