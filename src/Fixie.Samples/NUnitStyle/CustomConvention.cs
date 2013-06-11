using System;
using Fixie.Conventions;

namespace Fixie.Samples.NUnitStyle
{
    public class CustomConvention : Convention
    {
        readonly MethodFilter fixtureSetUp = Has<TestFixtureSetUpAttribute>();
        readonly MethodFilter fixtureTearDown = Has<TestFixtureTearDownAttribute>();
        readonly MethodFilter setUp = Has<SetUpAttribute>();
        readonly MethodFilter tearDown = Has<TearDownAttribute>();

        public CustomConvention()
        {
            Fixtures
                .HasOrInherits<TestFixtureAttribute>();

            Cases
                .HasOrInherits<TestAttribute>();

            FixtureExecution
                    .CreateInstancePerFixture();

            InstanceExecution
                .SetUpTearDown(fixtureSetUp, fixtureTearDown);

            CaseExecution
                .SetUpTearDown(setUp, tearDown);
        }

        static MethodFilter Has<TAttribute>() where TAttribute : Attribute
        {
            return new MethodFilter().HasOrInherits<TAttribute>();
        }
    }
}