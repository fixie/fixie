using System;
using System.Linq;
using System.Reflection;
using Fixie.Conventions;

namespace Fixie.Samples.xUnitStyle
{
    public class CustomConvention : Convention
    {
        public CustomConvention()
        {
            Fixtures
                .Where(HasAnyFactMethods);

            Cases
                .Where(method => method.HasOrInherits<FactAttribute>());
        }

        static bool HasAnyFactMethods(Type type)
        {
            return type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                       .Any(method => method.HasOrInherits<FactAttribute>());
        }
    }
}