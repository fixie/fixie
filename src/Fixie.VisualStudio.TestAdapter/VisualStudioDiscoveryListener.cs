using System;
using Fixie.Execution;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace Fixie.VisualStudio.TestAdapter
{
    public class VisualStudioDiscoveryListener : LongLivedMarshalByRefObject, IHandler<MethodGroupDiscovered>
    {
        readonly ITestCaseDiscoverySink discoverySink;
        readonly IMessageLogger log;
        readonly string assemblyPath;

        public VisualStudioDiscoveryListener(ITestCaseDiscoverySink discoverySink, IMessageLogger log, string assemblyPath)
        {
            this.discoverySink = discoverySink;
            this.log = log;
            this.assemblyPath = assemblyPath;
        }

        public void Handle(MethodGroupDiscovered message)
        {
            var sourceLocationProvider = new SourceLocationProvider(assemblyPath);

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
                log.Error(exception);
            }

            discoverySink.SendTestCase(testCase);
        }
    }
}