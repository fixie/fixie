using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fixie.Execution
{
    public class TraitFilterParser
    {
        public TraitFilter GetTraitFilter(ILookup<string, string> options)
        {
            var includedTraits = Parse(options, CommandLineOption.Include);
            var excludedTraits = Parse(options, CommandLineOption.Exclude);
            return new TraitFilter(includedTraits, excludedTraits);
        }

        static IEnumerable<Trait> Parse(ILookup<string, string> options, string optionKey)
        {
            if (!options.Contains(optionKey))
                yield break;

            foreach (var option in options[optionKey])
            {
                var kvps = option.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                                 .Select(x => x.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries))
                                 .ToArray();

                foreach (var kvp in kvps)
                {
                    if (kvp.Length == 2) continue;
                    var message = new StringBuilder()
                        .AppendFormat("Invalid option '{0} {1}'.", optionKey, option)
                        .AppendLine()
                        .Append("Valid format is key=value[;key=value].")
                        .ToString();
                    throw new FormatException(message);
                }

                foreach (var kvp in kvps)
                {
                    yield return new Trait(kvp[0], kvp[1]);
                }
            }
        }
    }
}