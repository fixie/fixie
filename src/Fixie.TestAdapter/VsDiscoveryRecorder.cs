using Fixie.Internal;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

namespace Fixie.TestAdapter;

class VsDiscoveryRecorder
{
    readonly ITestCaseDiscoverySink discoverySink;
    readonly string assemblyPath;

    public VsDiscoveryRecorder(ITestCaseDiscoverySink discoverySink, string assemblyPath)
    {
        this.discoverySink = discoverySink;
        this.assemblyPath = assemblyPath;
    }

    public void Record(PipeMessage.TestDiscovered testDiscovered)
    {
        var test = testDiscovered.Test;

        var discoveredTest = new TestCase(test, VsTestExecutor.Uri, assemblyPath)
        {
            DisplayName = test
        };

        discoverySink.SendTestCase(discoveredTest);
    }
}