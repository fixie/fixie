namespace Fixie.Tests
{
    public class ConventionTests
    {
        public void ShouldExecuteAllCasesInAllDiscoveredFixtures()
        {
            var listener = new StubListener();
            var convention = new DefaultConvention();

            convention.Execute(listener, typeof(SampleIrrelevantClass), typeof(int), typeof(SampleTests));

            listener.ShouldHaveEntries("Fixie.Tests.ConventionTests+SampleTests.PassA passed.",
                                       "Fixie.Tests.ConventionTests+SampleTests.PassB passed.");
        }

        class SampleIrrelevantClass
        {
            public void PassA() { }
            public void PassB() { }
        }

        class SampleTests
        {
            public void PassA() { }
            public void PassB() { }
        }
    }
}