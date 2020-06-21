namespace Fixie.Tests.TestAdapter
{
    using System.Collections.Generic;
    using System.IO;
    using Assertions;
    using Fixie.TestAdapter;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
    using static System.IO.Directory;

    public class DiscoveryListenerTests : MessagingTests
    {
        public void ShouldMapDiscoveredTestsToVsTestDiscoverySink()
        {
            var assemblyPath = typeof(MessagingTests).Assembly.Location;

            var log = new StubMessageLogger();
            var discoverySink = new StubTestCaseDiscoverySink();

            var listener = new DiscoveryListener(log, discoverySink, assemblyPath);

            Discover(listener, out var console);

            console.ShouldBeEmpty();

            log.Messages.ShouldBeEmpty();

            discoverySink.TestCases.Count.ShouldBe(6);
            discoverySink.TestCases[0]
                .ShouldBeDiscoveryTimeTest(TestClass + ".Fail", assemblyPath);
            discoverySink.TestCases[1]
                .ShouldBeDiscoveryTimeTest(TestClass + ".FailByAssertion", assemblyPath);
            discoverySink.TestCases[2]
                .ShouldBeDiscoveryTimeTest(TestClass + ".SkipWithoutReason", assemblyPath);
            discoverySink.TestCases[3]
                .ShouldBeDiscoveryTimeTest(TestClass + ".SkipWithReason", assemblyPath);
            discoverySink.TestCases[4]
                .ShouldBeDiscoveryTimeTestMissingSourceLocation(TestClass + ".Pass", assemblyPath);
            discoverySink.TestCases[5]
                .ShouldBeDiscoveryTimeTest(GenericTestClass + ".ShouldBeString", assemblyPath);
        }

        public void ShouldDefaultSourceLocationPropertiesWhenSourceInspectionThrows()
        {
            const string invalidAssemblyPath = "assembly.path.dll";

            var log = new StubMessageLogger();
            var discoverySink = new StubTestCaseDiscoverySink();

            var listener = new DiscoveryListener(log, discoverySink, invalidAssemblyPath);

            Discover(listener, out var console);

            console.ShouldBeEmpty();

            log.Messages.Count.ShouldBe(6);

            var expectedError =
                $"Error: {typeof(FileNotFoundException).FullName}: " +
                $"Could not find file '{Path.Combine(GetCurrentDirectory(), invalidAssemblyPath)}'.";
            log.Messages[0].Contains(expectedError).ShouldBe(true);
            log.Messages[1].Contains(expectedError).ShouldBe(true);
            log.Messages[2].Contains(expectedError).ShouldBe(true);
            log.Messages[3].Contains(expectedError).ShouldBe(true);
            log.Messages[4].Contains(expectedError).ShouldBe(true);
            log.Messages[5].Contains(expectedError).ShouldBe(true);

            discoverySink.TestCases.Count.ShouldBe(6);
            discoverySink.TestCases[0]
                .ShouldBeDiscoveryTimeTestMissingSourceLocation(TestClass + ".Fail", invalidAssemblyPath);
            discoverySink.TestCases[1]
                .ShouldBeDiscoveryTimeTestMissingSourceLocation(TestClass + ".FailByAssertion", invalidAssemblyPath);
            discoverySink.TestCases[2]
                .ShouldBeDiscoveryTimeTestMissingSourceLocation(TestClass + ".SkipWithoutReason", invalidAssemblyPath);
            discoverySink.TestCases[3]
                .ShouldBeDiscoveryTimeTestMissingSourceLocation(TestClass + ".SkipWithReason", invalidAssemblyPath);
            discoverySink.TestCases[4]
                .ShouldBeDiscoveryTimeTestMissingSourceLocation(TestClass + ".Pass", invalidAssemblyPath);
            discoverySink.TestCases[5]
                .ShouldBeDiscoveryTimeTestMissingSourceLocation(GenericTestClass + ".ShouldBeString", invalidAssemblyPath);
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