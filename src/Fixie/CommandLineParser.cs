using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Fixie
{
    public class CommandLineParser
    {
        public IEnumerable<string> AssemblyPaths { get; private set; }
        public IReadOnlyDictionary<string, string> CustomOptions { get; private set; }

        public CommandLineParser(params string[] args)
        {
            var queue = new Queue<string>(args);

            var assemblyPaths = new List<string>();
            var customOptions = new Dictionary<string, string>();

            while (queue.Any())
            {
                var item = queue.Dequeue();

                if (IsKey(item))
                {
                    if (!queue.Any() || IsKey(queue.Peek()))
                        throw new Exception(string.Format("Option {0} is missing its required value.", item));

                    var key = KeyName(item);
                    var value = queue.Dequeue();
                    
                    if (customOptions.ContainsKey(key))
                        throw new Exception(string.Format("Option {0} was specified twice.", item));

                    customOptions.Add(key, value);
                }
                else
                {
                    assemblyPaths.Add(item);
                }
            }

            AssemblyPaths = assemblyPaths.ToArray();
            CustomOptions = new ReadOnlyDictionary<string, string>(customOptions);
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