using Fixie.Conventions;

namespace Fixie.Samples.NUnitStyle
{
    public class CustomConvention : Convention
    {
        public CustomConvention()
        {
            Fixtures
                .Where(type => type.HasOrInherits<TestFixtureAttribute>());

            Cases
                .Where(method => method.HasOrInherits<TestAttribute>());
        }
    }
}