namespace Fixie.Tests.VisualStudio.TestAdapter
{
    using System.Collections.Generic;
    using System.Linq;
    using Fixie.Runner;
    using Fixie.Runner.Contracts;
    using Fixie.VisualStudio.TestAdapter;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using Newtonsoft.Json;
    using Should;

    public class VisualStudioDiscoveryMappingTests : MessagingTests
    {
        public void ShouldMapDiscoveredTestContractsToVisualStudioTypes()
        {
            string assemblyPath = typeof(MessagingTests).Assembly.Location;

            var sink = new StubDesignTimeSink();
            Discover(new DesignTimeDiscoveryListener(sink, assemblyPath));

            sink.LogEntries.ShouldBeEmpty();

            var testCases = DiscoveredTestCases(sink, assemblyPath);

            testCases.Length.ShouldEqual(5);
            testCases[0].ShouldBeDiscoveryTimeTestCase(assemblyPath, TestClass + ".Fail");
            testCases[1].ShouldBeDiscoveryTimeTestCase(assemblyPath, TestClass + ".FailByAssertion");
            testCases[2].ShouldBeDiscoveryTimeTestCase(assemblyPath, TestClass + ".Pass");
            testCases[3].ShouldBeDiscoveryTimeTestCase(assemblyPath, TestClass + ".SkipWithoutReason");
            testCases[4].ShouldBeDiscoveryTimeTestCase(assemblyPath, TestClass + ".SkipWithReason");
        }

        public void ShouldDefaultSourceLocationPropertiesWhenSourceInspectionThrows()
        {
            const string invalidAssemblyPath = "assembly.path.dll";

            var sink = new StubDesignTimeSink();
            Discover(new DesignTimeDiscoveryListener(sink, invalidAssemblyPath));

            sink.LogEntries.Count.ShouldEqual(5);

            var testCases = DiscoveredTestCases(sink, invalidAssemblyPath);

            testCases.Length.ShouldEqual(5);
            testCases[0].ShouldBeDiscoveryTimeTestCaseMissingSourceLocation(invalidAssemblyPath, TestClass+ ".Fail");
            testCases[1].ShouldBeDiscoveryTimeTestCaseMissingSourceLocation(invalidAssemblyPath, TestClass+ ".FailByAssertion");
            testCases[2].ShouldBeDiscoveryTimeTestCaseMissingSourceLocation(invalidAssemblyPath, TestClass+ ".Pass");
            testCases[3].ShouldBeDiscoveryTimeTestCaseMissingSourceLocation(invalidAssemblyPath, TestClass+ ".SkipWithoutReason");
            testCases[4].ShouldBeDiscoveryTimeTestCaseMissingSourceLocation(invalidAssemblyPath, TestClass+ ".SkipWithReason");
        }

        static TestCase[] DiscoveredTestCases(StubDesignTimeSink sink, string assemblyPath)
        {
            return sink.Messages
                .Select(jsonMessage => Payload<Test>(jsonMessage, "TestDiscovery.TestFound"))
                .OrderBy(x => x.FullyQualifiedName)
                .Select(x => x.ToVisualStudioType(assemblyPath))
                .ToArray();
        }

        static TExpectedPayload Payload<TExpectedPayload>(string jsonMessage, string expectedMessageType)
        {
            var message = JsonConvert.DeserializeObject<Message>(jsonMessage);

            message.MessageType.ShouldEqual(expectedMessageType);

            return message.Payload.ToObject<TExpectedPayload>();
        }

        class StubDesignTimeSink : IDesignTimeSink
        {
            public List<string> Messages { get; } = new List<string>();
            public List<string> LogEntries { get; } = new List<string>();

            public void Send(string message) => Messages.Add(message);
            public void Log(string message) => LogEntries.Add(message);
        }
    }
}