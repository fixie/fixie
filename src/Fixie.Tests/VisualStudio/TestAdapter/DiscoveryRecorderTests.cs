namespace Fixie.Tests.VisualStudio.TestAdapter
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Assertions;
    using Fixie.Execution.Listeners;
    using Fixie.VisualStudio.TestAdapter;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

    public class DiscoveryRecorderTests : MessagingTests
    {
        public void ShouldMapDiscoveredTestsToVisualStudioDiscoverySink()
        {
            var assemblyPath = typeof(MessagingTests).Assembly.Location;

            var log = new StubMessageLogger();
            var discoverySink = new StubTestCaseDiscoverySink();

            var discoveryRecorder = new DiscoveryRecorder(log, discoverySink, assemblyPath);

            var methodGroup = new MethodGroup(
                typeof(SampleTestClass),
                typeof(SampleTestClass).GetInstanceMethod("Fail"));
            
            discoveryRecorder.SendTestCase(new PipeListener.Test
            {
                FullyQualifiedName = methodGroup.FullName,
                DisplayName = methodGroup.FullName
            });

            log.Messages.ShouldBeEmpty();

            discoverySink.TestCases.Single()
                .ShouldBeDiscoveryTimeTest(methodGroup.FullName, assemblyPath);
        }

        public void ShouldDefaultSourceLocationPropertiesWhenSourceInspectionThrows()
        {
            const string invalidAssemblyPath = "assembly.path.dll";

            var log = new StubMessageLogger();
            var discoverySink = new StubTestCaseDiscoverySink();

            var discoveryRecorder = new DiscoveryRecorder(log, discoverySink, invalidAssemblyPath);

            var methodGroup = new MethodGroup(
                typeof(SampleTestClass),
                typeof(SampleTestClass).GetInstanceMethod("Fail"));

            discoveryRecorder.SendTestCase(new PipeListener.Test
            {
                FullyQualifiedName = methodGroup.FullName,
                DisplayName = methodGroup.FullName
            });

            log.Messages.Single().Contains(nameof(FileNotFoundException)).ShouldBeTrue();

            discoverySink.TestCases.Single()
                .ShouldBeDiscoveryTimeTestMissingSourceLocation(methodGroup.FullName, invalidAssemblyPath);
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