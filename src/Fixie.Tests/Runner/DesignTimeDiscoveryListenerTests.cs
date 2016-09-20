namespace Fixie.Tests.Runner
{
    using System.Collections.Generic;
    using System.Linq;
    using Assertions;
    using Fixie.Runner;
    using Fixie.Runner.Contracts;
    using Newtonsoft.Json;

    public class DesignTimeDiscoveryListenerTests : MessagingTests
    {
        public void ShouldReportDiscoveredMethodGroupsToDiscoverySink()
        {
            var assemblyPath = typeof(MessagingTests).Assembly.Location;

            var sink = new StubDesignTimeSink();
            Discover(new DesignTimeDiscoveryListener(sink, assemblyPath));

            sink.LogEntries.ShouldBeEmpty();

            var tests = DiscoveredTests(sink);

            tests.Length.ShouldEqual(5);
            tests[0].ShouldBeDiscoveryTimeTest(TestClass + ".Fail");
            tests[1].ShouldBeDiscoveryTimeTest(TestClass + ".FailByAssertion");
            tests[2].ShouldBeDiscoveryTimeTest(TestClass + ".Pass");
            tests[3].ShouldBeDiscoveryTimeTest(TestClass + ".SkipWithoutReason");
            tests[4].ShouldBeDiscoveryTimeTest(TestClass + ".SkipWithReason");
        }

        public void ShouldDefaultSourceLocationPropertiesWhenSourceInspectionThrows()
        {
            const string invalidAssemblyPath = "assembly.path.dll";

            var sink = new StubDesignTimeSink();
            Discover(new DesignTimeDiscoveryListener(sink, invalidAssemblyPath));

            sink.LogEntries.Count.ShouldEqual(5);

            var tests = DiscoveredTests(sink);

            tests.Length.ShouldEqual(5);
            tests[0].ShouldBeDiscoveryTimeTestMissingSourceLocation(TestClass + ".Fail");
            tests[1].ShouldBeDiscoveryTimeTestMissingSourceLocation(TestClass + ".FailByAssertion");
            tests[2].ShouldBeDiscoveryTimeTestMissingSourceLocation(TestClass + ".Pass");
            tests[3].ShouldBeDiscoveryTimeTestMissingSourceLocation(TestClass + ".SkipWithoutReason");
            tests[4].ShouldBeDiscoveryTimeTestMissingSourceLocation(TestClass + ".SkipWithReason");
        }

        static Test[] DiscoveredTests(StubDesignTimeSink sink)
        {
            return sink.Messages
                .Select(jsonMessage => Payload<Test>(jsonMessage, "TestDiscovery.TestFound"))
                .OrderBy(x => x.FullyQualifiedName)
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