using Fixie.Internal;
using Fixie.TestAdapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Fixie.Tests.Reports;

namespace Fixie.Tests.TestAdapter;

public class VsDiscoveryRecorderTests : MessagingTests
{
    public void ShouldMapDiscoveredTestsToVsTestDiscoverySink()
    {
        var assemblyPath = typeof(MessagingTests).Assembly.Location;

        var discoverySink = new StubTestCaseDiscoverySink();

        var vsDiscoveryRecorder = new VsDiscoveryRecorder(discoverySink, assemblyPath);

        RecordAnticipatedPipeMessages(vsDiscoveryRecorder);

        discoverySink.TestCases.ItemsShouldSatisfy([
            x => x.ShouldBeDiscoveryTimeTest(TestClass + ".Fail", assemblyPath),
            x => x.ShouldBeDiscoveryTimeTest(TestClass + ".FailByAssertion", assemblyPath),
            x => x.ShouldBeDiscoveryTimeTest(TestClass + ".Pass", assemblyPath),
            x => x.ShouldBeDiscoveryTimeTest(TestClass + ".Skip", assemblyPath),
            x => x.ShouldBeDiscoveryTimeTest(GenericTestClass + ".ShouldBeString", assemblyPath)
        ]);
    }

    void RecordAnticipatedPipeMessages(VsDiscoveryRecorder vsDiscoveryRecorder)
    {
        vsDiscoveryRecorder.Record(new PipeMessage.TestDiscovered
        {
            Test = TestClass + ".Fail"
        });

        vsDiscoveryRecorder.Record(new PipeMessage.TestDiscovered
        {
            Test = TestClass + ".FailByAssertion"
        });

        vsDiscoveryRecorder.Record(new PipeMessage.TestDiscovered
        {
            Test = TestClass + ".Pass"
        });

        vsDiscoveryRecorder.Record(new PipeMessage.TestDiscovered
        {
            Test = TestClass + ".Skip"
        });

        vsDiscoveryRecorder.Record(new PipeMessage.TestDiscovered
        {
            Test = GenericTestClass + ".ShouldBeString"
        });
    }

    class StubTestCaseDiscoverySink : ITestCaseDiscoverySink
    {
        public List<TestCase> TestCases { get; } = [];

        public void SendTestCase(TestCase discoveredTest)
            => TestCases.Add(discoveredTest);
    }
}