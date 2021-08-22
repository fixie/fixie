namespace Fixie.TestAdapter
{
    using System;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
    using Reports;

    class VsDiscoveryRecorder
    {
        readonly IMessageLogger log;
        readonly ITestCaseDiscoverySink discoverySink;
        readonly string assemblyPath;
        readonly SourceLocationProvider sourceLocationProvider;

        public VsDiscoveryRecorder(IMessageLogger log, ITestCaseDiscoverySink discoverySink, string assemblyPath)
        {
            this.log = log;
            this.discoverySink = discoverySink;
            this.assemblyPath = assemblyPath;

            sourceLocationProvider = new SourceLocationProvider(assemblyPath);
        }

        public void Record(PipeMessage.TestDiscovered testDiscovered)
        {
            var test = testDiscovered.Test;

            SourceLocation? sourceLocation = null;

            try
            {
                sourceLocationProvider.TryGetSourceLocation(test, out sourceLocation);
            }
            catch (Exception exception)
            {
                log.Error(exception.ToString());
            }

            var discoveredTest = new TestCase(test, VsTestExecutor.Uri, assemblyPath)
            {
                DisplayName = test
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