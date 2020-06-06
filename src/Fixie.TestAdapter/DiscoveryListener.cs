namespace Fixie.TestAdapter
{
    using System;
    using Internal;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

    class DiscoveryListener : Handler<MethodDiscovered>
    {
        readonly IMessageLogger log;
        readonly ITestCaseDiscoverySink discoverySink;
        readonly string assemblyPath;
        readonly SourceLocationProvider sourceLocationProvider;

        public DiscoveryListener(IMessageLogger log, ITestCaseDiscoverySink discoverySink, string assemblyPath)
        {
            this.log = log;
            this.discoverySink = discoverySink;
            this.assemblyPath = assemblyPath;

            sourceLocationProvider = new SourceLocationProvider(assemblyPath);
        }

        public void Handle(MethodDiscovered message)
        {
            var test = new Test(message.Method);

            SourceLocation? sourceLocation = null;

            try
            {
                sourceLocationProvider.TryGetSourceLocation(test.Class, test.Method, out sourceLocation);
            }
            catch (Exception exception)
            {
                log.Error(exception.ToString());
            }

            var discoveredTest = new TestCase(test.Name, VsTestExecutor.Uri, assemblyPath)
            {
                DisplayName = test.Name
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