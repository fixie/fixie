using Fixie.Internal;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace Fixie.TestAdapter;

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

        try
        {
            sourceLocationProvider.TryGetSourceLocation(test, out _);
        }
        catch (Exception exception)
        {
            log.Error(exception.ToString());
        }

        var discoveredTest = new TestCase(test, VsTestExecutor.Uri, assemblyPath)
        {
            DisplayName = test
        };

        discoverySink.SendTestCase(discoveredTest);
    }
}