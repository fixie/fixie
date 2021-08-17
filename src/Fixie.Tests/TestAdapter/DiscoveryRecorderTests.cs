namespace Fixie.Tests.TestAdapter
{
    using System.Collections.Generic;
    using System.IO;
    using Assertions;
    using Fixie.Reports;
    using Fixie.TestAdapter;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
    using Reports;
    using static System.IO.Directory;

    public class DiscoveryRecorderTests : MessagingTests
    {
        public void ShouldMapDiscoveredTestsToVsTestDiscoverySink()
        {
            var assemblyPath = typeof(MessagingTests).Assembly.Location;

            var log = new StubMessageLogger();
            var discoverySink = new StubTestCaseDiscoverySink();

            var discoveryRecorder = new DiscoveryRecorder(log, discoverySink, assemblyPath);

            RecordAnticipatedPipeMessages(discoveryRecorder);

            log.Messages.ShouldBeEmpty();

            discoverySink.TestCases.ShouldSatisfy(
                x => x.ShouldBeDiscoveryTimeTest(TestClass + ".Fail", assemblyPath),
                x => x.ShouldBeDiscoveryTimeTest(TestClass + ".FailByAssertion", assemblyPath),
                x => x.ShouldBeDiscoveryTimeTestMissingSourceLocation(TestClass + ".Pass", assemblyPath),
                x => x.ShouldBeDiscoveryTimeTest(TestClass + ".Skip", assemblyPath),
                x => x.ShouldBeDiscoveryTimeTest(GenericTestClass + ".ShouldBeString", assemblyPath));
        }

        public void ShouldDefaultSourceLocationPropertiesWhenSourceInspectionThrows()
        {
            const string invalidAssemblyPath = "assembly.path.dll";

            var log = new StubMessageLogger();
            var discoverySink = new StubTestCaseDiscoverySink();

            var discoveryRecorder = new DiscoveryRecorder(log, discoverySink, invalidAssemblyPath);

            RecordAnticipatedPipeMessages(discoveryRecorder);

            var expectedError =
                $"Error: {typeof(FileNotFoundException).FullName}: " +
                $"Could not find file '{Path.Combine(GetCurrentDirectory(), invalidAssemblyPath)}'.";
            log.Messages.ShouldSatisfy(
                x => x.Contains(expectedError).ShouldBe(true),
                x => x.Contains(expectedError).ShouldBe(true),
                x => x.Contains(expectedError).ShouldBe(true),
                x => x.Contains(expectedError).ShouldBe(true),
                x => x.Contains(expectedError).ShouldBe(true));

            discoverySink.TestCases.ShouldSatisfy(
                x => x.ShouldBeDiscoveryTimeTestMissingSourceLocation(TestClass + ".Fail", invalidAssemblyPath),
                x => x.ShouldBeDiscoveryTimeTestMissingSourceLocation(TestClass + ".FailByAssertion", invalidAssemblyPath),
                x => x.ShouldBeDiscoveryTimeTestMissingSourceLocation(TestClass + ".Pass", invalidAssemblyPath),
                x => x.ShouldBeDiscoveryTimeTestMissingSourceLocation(TestClass + ".Skip", invalidAssemblyPath),
                x => x.ShouldBeDiscoveryTimeTestMissingSourceLocation(GenericTestClass + ".ShouldBeString", invalidAssemblyPath));
        }

        void RecordAnticipatedPipeMessages(DiscoveryRecorder discoveryRecorder)
        {
            discoveryRecorder.Record(new PipeMessage.TestDiscovered
            {
                Test = TestClass + ".Fail"
            });

            discoveryRecorder.Record(new PipeMessage.TestDiscovered
            {
                Test = TestClass + ".FailByAssertion"
            });

            discoveryRecorder.Record(new PipeMessage.TestDiscovered
            {
                Test = TestClass + ".Pass"
            });

            discoveryRecorder.Record(new PipeMessage.TestDiscovered
            {
                Test = TestClass + ".Skip"
            });

            discoveryRecorder.Record(new PipeMessage.TestDiscovered
            {
                Test = GenericTestClass + ".ShouldBeString"
            });
        }

        class StubMessageLogger : IMessageLogger
        {
            public List<string> Messages { get; } = new List<string>();

            public void SendMessage(TestMessageLevel testMessageLevel, string message)
                => Messages.Add($"{testMessageLevel}: {message}");
        }

        class StubTestCaseDiscoverySink : ITestCaseDiscoverySink
        {
            public List<TestCase> TestCases { get; } = new List<TestCase>();

            public void SendTestCase(TestCase discoveredTest)
                => TestCases.Add(discoveredTest);
        }
    }
}