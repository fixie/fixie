using Fixie.Internal;
using Fixie.TestAdapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using Fixie.Tests.Reports;

namespace Fixie.Tests.TestAdapter;

public class VsDiscoveryRecorderTests : MessagingTests
{
    public void ShouldMapDiscoveredTestsToVsTestDiscoverySink()
    {
        var assemblyPath = typeof(MessagingTests).Assembly.Location;

        var log = new StubMessageLogger();
        var discoverySink = new StubTestCaseDiscoverySink();

        var vsDiscoveryRecorder = new VsDiscoveryRecorder(discoverySink, assemblyPath);

        RecordAnticipatedPipeMessages(assemblyPath, vsDiscoveryRecorder);

        log.Messages.ShouldMatch([]);

        discoverySink.TestCases.ItemsShouldSatisfy([
            x => x.ShouldBeDiscoveryTimeTest(TestClass + ".Fail", assemblyPath),
            x => x.ShouldBeDiscoveryTimeTest(TestClass + ".FailByAssertion", assemblyPath),
            x => x.ShouldBeDiscoveryTimeTestMissingSourceLocation(TestClass + ".Pass", assemblyPath),
            x => x.ShouldBeDiscoveryTimeTest(TestClass + ".Skip", assemblyPath),
            x => x.ShouldBeDiscoveryTimeTest(GenericTestClass + ".ShouldBeString", assemblyPath)
        ]);
    }

    void RecordAnticipatedPipeMessages(string assemblyPath, VsDiscoveryRecorder vsDiscoveryRecorder)
    {
        SourceLocationProvider sourceLocationProvider = new(assemblyPath);
        
        var test = TestClass + ".Fail";
        sourceLocationProvider.TryGetSourceLocation(test, out var sourceLocation).ShouldBe(true);
        vsDiscoveryRecorder.Record(new PipeMessage.TestDiscovered
        {
            Test = test,
            SourceLocation = sourceLocation
        });

        test = TestClass + ".FailByAssertion";
        sourceLocationProvider.TryGetSourceLocation(test, out sourceLocation).ShouldBe(true);
        vsDiscoveryRecorder.Record(new PipeMessage.TestDiscovered
        {
            Test = test,
            SourceLocation = sourceLocation
        });

        test = TestClass + ".Pass";
        sourceLocationProvider.TryGetSourceLocation(test, out sourceLocation).ShouldBe(false);
        sourceLocation.ShouldBe(null);
        vsDiscoveryRecorder.Record(new PipeMessage.TestDiscovered
        {
            Test = test,
            SourceLocation = sourceLocation
        });

        test = TestClass + ".Skip";
        sourceLocationProvider.TryGetSourceLocation(test, out sourceLocation).ShouldBe(true);
        vsDiscoveryRecorder.Record(new PipeMessage.TestDiscovered
        {
            Test = test,
            SourceLocation = sourceLocation
        });

        test = GenericTestClass + ".ShouldBeString";
        sourceLocationProvider.TryGetSourceLocation(test, out sourceLocation).ShouldBe(true);
        vsDiscoveryRecorder.Record(new PipeMessage.TestDiscovered
        {
            Test = test,
            SourceLocation = sourceLocation
        });
    }

    class StubMessageLogger : IMessageLogger
    {
        public List<string> Messages { get; } = [];

        public void SendMessage(TestMessageLevel testMessageLevel, string message)
            => Messages.Add($"{testMessageLevel}: {message}");
    }

    class StubTestCaseDiscoverySink : ITestCaseDiscoverySink
    {
        public List<TestCase> TestCases { get; } = [];

        public void SendTestCase(TestCase discoveredTest)
            => TestCases.Add(discoveredTest);
    }
}