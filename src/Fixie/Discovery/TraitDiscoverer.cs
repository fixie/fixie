using Fixie.Conventions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fixie.Discovery
{
    public class TraitDiscoverer
    {
        readonly TraitSource[] traitSources;

        public TraitDiscoverer(Configuration config)
        {
            traitSources = config.TraitSourceTypes
                .Select(sourceType => (TraitSource)Activator.CreateInstance(sourceType))
                .ToArray();
        }

        public IEnumerable<Trait> GetTraits(MethodInfo method)
        {
            return traitSources.SelectMany(source => source.GetTraits(method));
        }
    }
}