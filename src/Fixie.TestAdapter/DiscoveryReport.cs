namespace Fixie.TestAdapter
{
    using System;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
    using Reports;

    class DiscoveryReport : Handler<TestDiscovered>
    {
        readonly IMessageLogger log;
        readonly ITestCaseDiscoverySink discoverySink;
        readonly string assemblyPath;
        readonly SourceLocationProvider sourceLocationProvider;

        public DiscoveryReport(IMessageLogger log, ITestCaseDiscoverySink discoverySink, string assemblyPath)
        {
            this.log = log;
            this.discoverySink = discoverySink;
            this.assemblyPath = assemblyPath;

            sourceLocationProvider = new SourceLocationProvider(assemblyPath);
        }

        public void Handle(TestDiscovered message)
        {
            var test = message.Test;

            SourceLocation? sourceLocation = null;

            try
            {
                sourceLocationProvider.TryGetSourceLocation(test.Class, test.Method, out sourceLocation);
            }
            catch (Exception exception)
            {
                log.Error(exception.ToString());
            }

            var discoveredTest = new TestCase(test.FullName, VsTestExecutor.Uri, assemblyPath)
            {
                DisplayName = test.FullName
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