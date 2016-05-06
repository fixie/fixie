using System;
using Fixie.Execution;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace Fixie.VisualStudio.TestAdapter
{
    public class VisualStudioDiscoveryListener
    {
        readonly IMessageLogger log;
        readonly ITestCaseDiscoverySink discoverySink;
        readonly string assemblyPath;
        readonly SourceLocationProvider sourceLocationProvider;

        public VisualStudioDiscoveryListener(IMessageLogger log, ITestCaseDiscoverySink discoverySink, string assemblyPath)
        {
            this.log = log;
            this.discoverySink = discoverySink;
            this.assemblyPath = assemblyPath;
            this.sourceLocationProvider = new SourceLocationProvider(assemblyPath);
        }

        public void DiscoverMethodGroups(ExecutionEnvironment environment)
        {
            var methodGroups = environment.DiscoverTestMethodGroups(new Options());

            foreach (var methodGroup in methodGroups)
                Handle(methodGroup);
        }

        void Handle(MethodGroup methodGroup)
        {
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