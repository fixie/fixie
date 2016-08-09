namespace Fixie.Runner
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Custom options made available to test runners and custom conventions.
    /// For any given key, this collects zero or more values.
    /// </summary>
    public class Options
    {
        readonly IDictionary<string, List<string>> options = new Dictionary<string, List<string>>();

        /// <summary>
        /// Gets the number of keys in the Options set.
        /// </summary>
        public int Count => options.Count;

        /// <summary>
        /// Gets all the defined keys in the Options set.
        /// </summary>
        public IReadOnlyList<string> Keys => options.Keys.ToArray();

        /// <summary>
        /// Determines whether the Options set contains any elements with the specified key.
        /// </summary>
        public bool Contains(string key) => options.ContainsKey(key);

        /// <summary>
        /// Adds the specified value to the Options set, under the specified key.
        /// If multiple values are added for the same key, all of those values are included in the set.
        /// </summary>
        public void Add(string key, string value)
        {
            if (!Contains(key))
                options[key] = new List<string>();

            options[key].Add(value);
        }

        /// <summary>
        /// Gets all of the Options set values with the specified key.
        /// </summary>
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