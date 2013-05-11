using Should;

namespace Fixie.Tests
{
    public class ClassFixtureTests
    {
        public void ShouldBeNamedAfterTheGivenFixtureClass()
        {
            var fixture = new ClassFixture(typeof(SampleFixture), new DefaultConvention());

            fixture.Name.ShouldEqual("Fixie.Tests.ClassFixtureTests+SampleFixture");
        }

        class SampleFixture { }
    }
}