namespace Fixie.Tests.VisualStudio.TestAdapter
{
    using System.Collections.Generic;
    using System.Linq;
    using Assertions;
    using Execution;
    using Fixie.VisualStudio.TestAdapter;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

    public class VisualStudioDiscoveryListenerTests : MessagingTests
    {
        public void ShouldAllowRunnersInOtherAppDomainsToReportTestDiscoveryToVisualStudio()
        {
            typeof(IDiscoveryRecorder).ShouldBeSafeAppDomainCommunicationInterface();
        }

        public void ShouldReportDiscoveredMethodsToDiscoverySink()
        {
            var assemblyPath = typeof(MessagingTests).Assembly.Location;

            var log = new StubMessageLogger();
            var discoverySink = new StubTestCaseDiscoverySink();

            using (var discoveryRecorder = new DiscoveryRecorder(log, discoverySink, assemblyPath))
                Discover(new VisualStudioDiscoveryListener(discoveryRecorder, assemblyPath));

            log.Messages.ShouldBeEmpty();

            var tests = DiscoveredTests(discoverySink);

            tests.Length.ShouldEqual(5);
            tests[0].ShouldBeDiscoveryTimeTest(TestClass + ".Fail", assemblyPath);
            tests[1].ShouldBeDiscoveryTimeTest(TestClass + ".FailByAssertion", assemblyPath);
            tests[2].ShouldBeDiscoveryTimeTest(TestClass + ".Pass", assemblyPath);
            tests[3].ShouldBeDiscoveryTimeTest(TestClass + ".SkipWithoutReason", assemblyPath);
            tests[4].ShouldBeDiscoveryTimeTest(TestClass + ".SkipWithReason", assemblyPath);
        }

        public void ShouldDefaultSourceLocationPropertiesWhenSourceInspectionThrows()
        {
            const string invalidAssemblyPath = "assembly.path.dll";

            var log = new StubMessageLogger();
            var discoverySink = new StubTestCaseDiscoverySink();

            using (var discoveryRecorder = new DiscoveryRecorder(log, discoverySink, invalidAssemblyPath))
                Discover(new VisualStudioDiscoveryListener(discoveryRecorder, invalidAssemblyPath));

            log.Messages.Count.ShouldEqual(5);

            var tests = DiscoveredTests(discoverySink);

            tests.Length.ShouldEqual(5);
            tests[0].ShouldBeDiscoveryTimeTestMissingSourceLocation(TestClass + ".Fail", invalidAssemblyPath);
            tests[1].ShouldBeDiscoveryTimeTestMissingSourceLocation(TestClass + ".FailByAssertion", invalidAssemblyPath);
            tests[2].ShouldBeDiscoveryTimeTestMissingSourceLocation(TestClass + ".Pass", invalidAssemblyPath);
            tests[3].ShouldBeDiscoveryTimeTestMissingSourceLocation(TestClass + ".SkipWithoutReason", invalidAssemblyPath);
            tests[4].ShouldBeDiscoveryTimeTestMissingSourceLocation(TestClass + ".SkipWithReason", invalidAssemblyPath);
        }

        static TestCase[] DiscoveredTests(StubTestCaseDiscoverySink discoverySink)
            => discoverySink
                .TestCases
                .OrderBy(x => x.FullyQualifiedName)
                .ToArray();

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