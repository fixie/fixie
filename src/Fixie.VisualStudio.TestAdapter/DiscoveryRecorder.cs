namespace Fixie.VisualStudio.TestAdapter
{
    using System;
    using Execution;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

    public interface IDiscoveryRecorder
    {
        void Error(string message);
        void SendTestFound(string fullyQualifiedName, string displayName, string codeFilePath, int lineNumber);
        void SendTestFound(string fullyQualifiedName, string displayName);
    }

    public class DiscoveryRecorder :
#if NET452
        LongLivedMarshalByRefObject,
#else
        IDisposable,
#endif
        IDiscoveryRecorder
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

#if !NET452
        public void Dispose() { }
#endif
    }
}