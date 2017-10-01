namespace Fixie.VisualStudio.TestAdapter
{
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

    public class DiscoveryRecorder
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

        public void SendTestFound(string fullyQualifiedName, string displayName, string codeFilePath, int lineNumber)
        {
            discoverySink.SendTestCase(new TestCase(fullyQualifiedName, VsTestExecutor.Uri, assemblyPath)
            {
                DisplayName = displayName,
                CodeFilePath = codeFilePath,
                LineNumber = lineNumber
            });
        }

        public void SendTestFound(string fullyQualifiedName, string displayName)
        {
            discoverySink.SendTestCase(new TestCase(fullyQualifiedName, VsTestExecutor.Uri, assemblyPath)
            {
                DisplayName = displayName,
            });
        }
    }
}