using System.Collections.Generic;
using System.Reflection;

namespace Fixie
{
    public interface TraitSource
    {
        IEnumerable<Trait> GetTraits(MethodInfo method);
    }
}