using System;
using System.Linq;
using Fixie.Conventions;

namespace Fixie.Samples.xUnitStyle
{
    public class CustomConvention : Convention
    {
        readonly MethodFilter factMethods = new MethodFilter().HasOrInherits<FactAttribute>();

        public CustomConvention()
        {
            Fixtures
                .Where(HasAnyFactMethods);

            Cases
                .HasOrInherits<FactAttribute>();
        }

        bool HasAnyFactMethods(Type type)
        {
            return factMethods.Filter(type).Any();
        }
    }
}