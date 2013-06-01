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

            InstanceExecutionBehavior =
                new InstanceBehaviorBuilder()
                    .SetUpTearDown<TestFixtureSetUpAttribute, TestFixtureTearDownAttribute>()
                    .Behavior;

            CaseExecution
                .SetUpTearDown<SetUpAttribute, TearDownAttribute>();
        }
    }
}