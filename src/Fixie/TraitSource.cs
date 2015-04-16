using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Fixie
{
    /// <summary>
    /// Defines a source of traits for a test method.
    /// </summary>
    public interface TraitSource
    {
        IEnumerable<Trait> GetTraits(MethodInfo methodInfo);
    }

    /// <summary>
    /// Defines a source of traits for a test method.
    /// </summary>
    /// <param name="method"></param>
    /// <returns></returns>
    public delegate IEnumerable<Trait> TraitSourceFunc   (MethodInfo method);
}
