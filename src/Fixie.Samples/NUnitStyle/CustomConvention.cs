using Fixie.Behaviors;
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

            FixtureExecutionBehavior = new CreateInstancePerFixture();

            InstanceExecutionBehavior =
                InstanceExecutionBehavior
                    .Wrap<TestFixtureSetUpAttribute, TestFixtureTearDownAttribute>();

            CaseExecutionBehavior =
                CaseExecutionBehavior
                    .Wrap<SetUpAttribute, TearDownAttribute>();
        }
    }
}