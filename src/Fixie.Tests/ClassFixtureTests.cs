using Should;

namespace Fixie.Tests
{
    public class ClassFixtureTests
    {
        public void ShouldBeNamedAfterTheGivenFixtureClass()
        {
            var defaultConvention = new DefaultConvention();
            var fixtureClass = typeof(SampleFixture);
            var fixture = new ClassFixture(fixtureClass, defaultConvention);

            fixture.Name.ShouldEqual("Fixie.Tests.ClassFixtureTests+SampleFixture");
        }

        class SampleFixture { }
    }
}