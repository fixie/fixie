namespace Fixie.VisualStudio.TestAdapter
{
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

    public class TestCaseDiscoverySink
    {
        readonly ITestCaseDiscoverySink discoverySink;

        public TestCaseDiscoverySink(ITestCaseDiscoverySink discoverySink)
        {
            this.discoverySink = discoverySink;
        }

        public void SendTestCase(TestCase discoveredTest)
        {
            discoverySink.SendTestCase(discoveredTest);
        }
    }
}