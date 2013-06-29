using System;
using Fixie.Conventions;

namespace Fixie.Samples.NUnitStyle
{
    public class CustomConvention : Convention
    {
        public CustomConvention()
        {
            Classes
                .HasOrInherits<TestFixtureAttribute>();

            Cases
                .HasOrInherits<TestAttribute>();

            ClassExecution
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