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

            FixtureExecutionBehavior =
                new TypeBehaviorBuilder()
                    .CreateInstancePerFixture()
                    .Behavior;

            InstanceExecution
                .SetUpTearDown<TestFixtureSetUpAttribute, TestFixtureTearDownAttribute>();

            CaseExecution
                .SetUpTearDown<SetUpAttribute, TearDownAttribute>();
        }
    }
}