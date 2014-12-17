using System;
using System.Collections.Generic;

namespace Fixie
{
    /// <summary>
    /// Subclass TestAssembly within your test assembly in order to explicitly list which conventions to apply.
    /// </summary>
    public class TestAssembly
    {
        readonly List<Type> conventionTypes = new List<Type>();

        /// <summary>
        /// Identifies the given convention type as being applicable to this test assembly.
        /// </summary>
        public void Apply<TConvention>() where TConvention : Convention
        {
            conventionTypes.Add(typeof(TConvention));
        }

        public IReadOnlyList<Type> ConventionTypes { get { return conventionTypes; } }
    }
}