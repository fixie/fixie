using System.Collections.Generic;
using System.Linq;

namespace Fixie.Execution
{
    public class TraitFilter
    {
        readonly Trait[] includedTraits;
        readonly Trait[] excludedTraits;

        public TraitFilter(IEnumerable<Trait> includedTraits, IEnumerable<Trait> excludedTraits)
        {
            this.includedTraits = includedTraits.ToArray();
            this.excludedTraits = excludedTraits.ToArray();
        }

        public bool IsMatch(IReadOnlyCollection<Trait> traits)
        {
            return IsIncluded(traits) && !IsExcluded(traits);
        }

        bool IsIncluded(IReadOnlyCollection<Trait> traits)
        {
            return !includedTraits.Any() || IsMatch(includedTraits, traits);
        }

        bool IsExcluded(IReadOnlyCollection<Trait> traits)
        {
            return excludedTraits.Any() && IsMatch(excludedTraits, traits);
        }

        static bool IsMatch(IEnumerable<Trait> traits1, IReadOnlyCollection<Trait> traits2)
        {
            foreach (var trait1 in traits1)
            {
                foreach (var trait2 in traits2)
                {
                    if (trait1.Key == trait2.Key && trait1.Value == trait2.Value)
                        return true;
                }
            }

            return false;
        }
    }
}