namespace Fixie.VisualStudio.TestAdapter
{
    using System;
    using Execution.Listeners;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

    public class DiscoveryRecorder
    {
        readonly IMessageLogger log;
        readonly ITestCaseDiscoverySink discoverySink;
        readonly string assemblyPath;
        readonly SourceLocationProvider sourceLocationProvider;

        public DiscoveryRecorder(IMessageLogger log, ITestCaseDiscoverySink discoverySink, string assemblyPath)
        {
            this.log = log;
            this.discoverySink = discoverySink;
            this.assemblyPath = assemblyPath;

            sourceLocationProvider = new SourceLocationProvider(assemblyPath);
        }

        public void SendTestCase(PipeMessage.Test test)
        {
            SourceLocation sourceLocation = null;

            try
            {
                var methodGroup = new MethodGroup(test.FullyQualifiedName);
                sourceLocationProvider.TryGetSourceLocation(methodGroup, out sourceLocation);
            }
            catch (Exception exception)
            {
                log.Error(exception.ToString());
            }

            var discoveredTest = new TestCase(test.FullyQualifiedName, VsTestExecutor.Uri, assemblyPath)
            {
                DisplayName = test.FullyQualifiedName
            };

            if (sourceLocation != null)
            {
                discoveredTest.CodeFilePath = sourceLocation.CodeFilePath;
                discoveredTest.LineNumber = sourceLocation.LineNumber;
            }

            discoverySink.SendTestCase(discoveredTest);
        }
    }
}