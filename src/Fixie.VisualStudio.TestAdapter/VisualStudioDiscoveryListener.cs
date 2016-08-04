namespace Fixie.VisualStudio.TestAdapter
{
    using System;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using Execution;

    public class VisualStudioDiscoveryListener : LongLivedMarshalByRefObject, Handler<MethodGroupDiscovered>
    {
        readonly MessageLogger log;
        readonly TestCaseDiscoverySink discoverySink;
        readonly string assemblyPath;
        readonly SourceLocationProvider sourceLocationProvider;

        public VisualStudioDiscoveryListener(MessageLogger log, TestCaseDiscoverySink discoverySink, string assemblyPath)
        {
            this.log = log;
            this.discoverySink = discoverySink;
            this.assemblyPath = assemblyPath;
            sourceLocationProvider = new SourceLocationProvider(assemblyPath);
        }

        public void Handle(MethodGroupDiscovered message)
        {
            var methodGroup = message.MethodGroup;

            var testCase = new TestCase(methodGroup.FullName, VsTestExecutor.Uri, assemblyPath);

            try
            {
                SourceLocation sourceLocation;
                if (sourceLocationProvider.TryGetSourceLocation(methodGroup, out sourceLocation))
                {
                    testCase.CodeFilePath = sourceLocation.CodeFilePath;
                    testCase.LineNumber = sourceLocation.LineNumber;
                }
            }
            catch (Exception exception)
            {
                log.Error(exception.ToString());
            }

            discoverySink.SendTestCase(testCase);
        }
    }
}