using Fixie.Internal;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

namespace Fixie.TestAdapter;

class VsDiscoveryRecorder(ITestCaseDiscoverySink discoverySink, string assemblyPath)
{
    public void Record(PipeMessage.TestDiscovered testDiscovered)
    {
        var test = testDiscovered.Test;

        var discoveredTest = new TestCase(test, VsTestExecutor.Uri, assemblyPath)
        {
            DisplayName = test
        };

        var sourceLocation = testDiscovered.SourceLocation;

        if (sourceLocation != null)
        {
            discoveredTest.CodeFilePath = sourceLocation.CodeFilePath;
            discoveredTest.LineNumber = sourceLocation.LineNumber;
        }

        discoverySink.SendTestCase(discoveredTest);
    }
}