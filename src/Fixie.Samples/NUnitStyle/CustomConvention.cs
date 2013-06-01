using Fixie.Conventions;

namespace Fixie.Samples.NUnitStyle
{
    public class CustomConvention : Convention
    {
        public CustomConvention()
        {
            Fixtures
                .HasOrInherits<TestFixtureAttribute>();

            Cases
                .HasOrInherits<TestAttribute>();

            FixtureExecution
                    .CreateInstancePerFixture();

            InstanceExecution
                .SetUpTearDown<TestFixtureSetUpAttribute, TestFixtureTearDownAttribute>();

            CaseExecution
                .SetUpTearDown<SetUpAttribute, TearDownAttribute>();
        }
    }
}