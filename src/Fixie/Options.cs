using System;
using System.Collections.Generic;
using System.Linq;

namespace Fixie
{
    [Serializable]
    public class Options
    {
        readonly IDictionary<string, List<string>> options = new Dictionary<string, List<string>>();

        public int Count
        {
            get { return options.Count; }
        }

        public IReadOnlyList<string> Keys
        {
            get { return options.Keys.ToArray(); }
        }

        public bool Contains(string key)
        {
            return options.ContainsKey(key);
        }

        public void Add(string key, string value)
        {
            if (!Contains(key))
                options[key] = new List<string>();

            options[key].Add(value);
        }

        public IReadOnlyList<string> this[string key]
        {
            get
            {
                if (!Contains(key))
                    return new string[] { };

                return options[key];
            }
        }
    }
}