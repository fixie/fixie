namespace Fixie.VisualStudio.TestAdapter.Wrappers
{
    using Execution;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

    public class TestCaseDiscoverySink : LongLivedMarshalByRefObject
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