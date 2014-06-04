using System;
using System.Collections.Generic;

namespace Fixie.Conventions
{
    public class TestAssembly
    {
        readonly List<Type> conventionTypes = new List<Type>();

        public void Apply<TConvention>() where TConvention : Convention
        {
            conventionTypes.Add(typeof(TConvention));
        }

        public IReadOnlyList<Type> ConventionTypes { get { return conventionTypes; } }
    }
}