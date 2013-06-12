using System;
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
                .SetUpTearDown(Has<TestFixtureSetUpAttribute>(), Has<TestFixtureTearDownAttribute>());

            CaseExecution
                .SetUpTearDown(Has<SetUpAttribute>(), Has<TearDownAttribute>());
        }

        static MethodFilter Has<TAttribute>() where TAttribute : Attribute
        {
            return new MethodFilter().HasOrInherits<TAttribute>();
        }
    }
}