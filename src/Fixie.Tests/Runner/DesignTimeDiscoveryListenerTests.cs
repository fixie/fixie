namespace Fixie.Tests.Runner
{
    using System.Collections.Generic;
    using System.Linq;
    using Fixie.Runner;
    using Microsoft.Extensions.Testing.Abstractions;
    using Newtonsoft.Json;
    using Should;

    public class DesignTimeDiscoveryListenerTests : MessagingTests
    {
        public void ShouldReportDiscoveredMethodGroupsToDiscoverySink()
        {
            var sink = new StubDesignTimeSink();
            Discover(new DesignTimeDiscoveryListener(sink));

            sink.LogEntries.ShouldBeEmpty();

            var tests = new List<Test>();
            foreach (var jsonMessage in sink.Messages)
            {
                var message = JsonConvert.DeserializeObject<Message>(jsonMessage);

                message.MessageType.ShouldEqual("TestDiscovery.TestFound");

                tests.Add(message.Payload.ToObject<Test>());
            }

            tests = tests.OrderBy(x => x.FullyQualifiedName).ToList();

            foreach (var test in tests)
            {
                test.Id.ShouldEqual(null);
                test.DisplayName.ShouldEqual(test.FullyQualifiedName);
                test.Properties.ShouldBeEmpty();

                //TODO: Discover source location.
                test.CodeFilePath.ShouldEqual(null);
                test.LineNumber.ShouldEqual(null);
                //testCase.CodeFilePath.EndsWith("MessagingTests.cs").ShouldBeTrue();
                //testCase.LineNumber.ShouldBeGreaterThan(0);
            }

            tests.Select(x => x.FullyQualifiedName)
                .ShouldEqual(
                    TestClass + ".Fail",
                    TestClass + ".FailByAssertion",
                    TestClass + ".Pass",
                    TestClass + ".SkipWithoutReason",
                    TestClass + ".SkipWithReason");
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