namespace Fixie.VisualStudio.TestAdapter.Wrappers
{
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

    public class TestCaseDiscoverySink
    {
        readonly ITestCaseDiscoverySink discoverySink;

        public TestCaseDiscoverySink(ITestCaseDiscoverySink discoverySink)
        {
            this.discoverySink = discoverySink;
        }

        public void SendTestCase(TestCaseModel discoveredTest)
            => discoverySink.SendTestCase(discoveredTest.ToVisualStudioType());
    }
}