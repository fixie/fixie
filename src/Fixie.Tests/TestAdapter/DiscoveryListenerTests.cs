﻿namespace Fixie.Tests.TestAdapter
{
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using Assertions;
    using Fixie.TestAdapter;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
    using Reports;
    using static System.IO.Directory;

    public class DiscoveryListenerTests : MessagingTests
    {
        public async Task ShouldMapDiscoveredTestsToVsTestDiscoverySink()
        {
            var assemblyPath = typeof(MessagingTests).Assembly.Location;

            var log = new StubMessageLogger();
            var discoverySink = new StubTestCaseDiscoverySink();

            var listener = new DiscoveryListener(log, discoverySink, assemblyPath);

            await DiscoverAsync(listener);

            log.Messages.ShouldBeEmpty();

            discoverySink.TestCases.ShouldSatisfy(
                x => x.ShouldBeDiscoveryTimeTest(TestClass + ".Fail", assemblyPath),
                x => x.ShouldBeDiscoveryTimeTest(TestClass + ".FailByAssertion", assemblyPath),
                x => x.ShouldBeDiscoveryTimeTestMissingSourceLocation(TestClass + ".Pass", assemblyPath),
                x => x.ShouldBeDiscoveryTimeTest(TestClass + ".SkipWithReason", assemblyPath),
                x => x.ShouldBeDiscoveryTimeTest(TestClass + ".SkipWithoutReason", assemblyPath),
                x => x.ShouldBeDiscoveryTimeTest(GenericTestClass + ".ShouldBeString", assemblyPath));
        }

        public async Task ShouldDefaultSourceLocationPropertiesWhenSourceInspectionThrows()
        {
            const string invalidAssemblyPath = "assembly.path.dll";

            var log = new StubMessageLogger();
            var discoverySink = new StubTestCaseDiscoverySink();

            var listener = new DiscoveryListener(log, discoverySink, invalidAssemblyPath);

            await DiscoverAsync(listener);

            var expectedError =
                $"Error: {typeof(FileNotFoundException).FullName}: " +
                $"Could not find file '{Path.Combine(GetCurrentDirectory(), invalidAssemblyPath)}'.";
            log.Messages.ShouldSatisfy(
                x => x.Contains(expectedError).ShouldBe(true),
                x => x.Contains(expectedError).ShouldBe(true),
                x => x.Contains(expectedError).ShouldBe(true),
                x => x.Contains(expectedError).ShouldBe(true),
                x => x.Contains(expectedError).ShouldBe(true),
                x => x.Contains(expectedError).ShouldBe(true));

            discoverySink.TestCases.ShouldSatisfy(
                x => x.ShouldBeDiscoveryTimeTestMissingSourceLocation(TestClass + ".Fail", invalidAssemblyPath),
                x => x.ShouldBeDiscoveryTimeTestMissingSourceLocation(TestClass + ".FailByAssertion", invalidAssemblyPath),
                x => x.ShouldBeDiscoveryTimeTestMissingSourceLocation(TestClass + ".Pass", invalidAssemblyPath),
                x => x.ShouldBeDiscoveryTimeTestMissingSourceLocation(TestClass + ".SkipWithReason", invalidAssemblyPath),
                x => x.ShouldBeDiscoveryTimeTestMissingSourceLocation(TestClass + ".SkipWithoutReason", invalidAssemblyPath),
                x => x.ShouldBeDiscoveryTimeTestMissingSourceLocation(GenericTestClass + ".ShouldBeString", invalidAssemblyPath));
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