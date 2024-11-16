using Fixie.Internal;
using Fixie.TestAdapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using Fixie.Tests.Reports;
using static System.IO.Directory;

namespace Fixie.Tests.TestAdapter;

public class VsDiscoveryRecorderTests : MessagingTests
{
    public void ShouldMapDiscoveredTestsToVsTestDiscoverySink()
    {
        var assemblyPath = typeof(MessagingTests).Assembly.Location;

        var log = new StubMessageLogger();
        var discoverySink = new StubTestCaseDiscoverySink();

        var vsDiscoveryRecorder = new VsDiscoveryRecorder(log, discoverySink, assemblyPath);

        RecordAnticipatedPipeMessages(vsDiscoveryRecorder);

        log.Messages.ShouldMatch([]);

        discoverySink.TestCases.ItemsShouldSatisfy([
            x => x.ShouldBeDiscoveryTimeTestMissingSourceLocation(TestClass + ".Fail", assemblyPath),
            x => x.ShouldBeDiscoveryTimeTestMissingSourceLocation(TestClass + ".FailByAssertion", assemblyPath),
            x => x.ShouldBeDiscoveryTimeTestMissingSourceLocation(TestClass + ".Pass", assemblyPath),
            x => x.ShouldBeDiscoveryTimeTestMissingSourceLocation(TestClass + ".Skip", assemblyPath),
            x => x.ShouldBeDiscoveryTimeTestMissingSourceLocation(GenericTestClass + ".ShouldBeString", assemblyPath)
        ]);
    }

    public void ShouldDefaultSourceLocationPropertiesWhenSourceInspectionThrows()
    {
        const string invalidAssemblyPath = "assembly.path.dll";

        var log = new StubMessageLogger();
        var discoverySink = new StubTestCaseDiscoverySink();

        var discoveryRecorder = new VsDiscoveryRecorder(log, discoverySink, invalidAssemblyPath);

        RecordAnticipatedPipeMessages(discoveryRecorder);

        var expectedError =
            $"Error: {typeof(FileNotFoundException).FullName}: " +
            $"Could not find file '{Path.Combine(GetCurrentDirectory(), invalidAssemblyPath)}'.";
        log.Messages.ItemsShouldSatisfy([
            x => x.Contains(expectedError).ShouldBe(true),
            x => x.Contains(expectedError).ShouldBe(true),
            x => x.Contains(expectedError).ShouldBe(true),
            x => x.Contains(expectedError).ShouldBe(true),
            x => x.Contains(expectedError).ShouldBe(true)
        ]);

        discoverySink.TestCases.ItemsShouldSatisfy([
            x => x.ShouldBeDiscoveryTimeTestMissingSourceLocation(TestClass + ".Fail", invalidAssemblyPath),
            x => x.ShouldBeDiscoveryTimeTestMissingSourceLocation(TestClass + ".FailByAssertion", invalidAssemblyPath),
            x => x.ShouldBeDiscoveryTimeTestMissingSourceLocation(TestClass + ".Pass", invalidAssemblyPath),
            x => x.ShouldBeDiscoveryTimeTestMissingSourceLocation(TestClass + ".Skip", invalidAssemblyPath),
            x => x.ShouldBeDiscoveryTimeTestMissingSourceLocation(GenericTestClass + ".ShouldBeString", invalidAssemblyPath)
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