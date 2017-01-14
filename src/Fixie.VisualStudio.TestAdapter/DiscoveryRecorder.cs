namespace Fixie.VisualStudio.TestAdapter
{
    using Execution;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

    public class DiscoveryRecorder : LongLivedMarshalByRefObject
    {
        readonly IMessageLogger log;
        readonly ITestCaseDiscoverySink discoverySink;
        readonly string assemblyPath;

        public DiscoveryRecorder(IMessageLogger log, ITestCaseDiscoverySink discoverySink, string assemblyPath)
        {
            this.log = log;
            this.discoverySink = discoverySink;
            this.assemblyPath = assemblyPath;
        }

        public void Error(string message) => log.Error(message);

        public void SendTestFound(Test test)
        {
            var testCase = new TestCase(test.FullyQualifiedName, VsTestExecutor.Uri, assemblyPath)
            {
                DisplayName = test.DisplayName,
                CodeFilePath = test.CodeFilePath
            };

            if (test.LineNumber != null)
                testCase.LineNumber = test.LineNumber.Value;

            discoverySink.SendTestCase(testCase);
        }
    }
}