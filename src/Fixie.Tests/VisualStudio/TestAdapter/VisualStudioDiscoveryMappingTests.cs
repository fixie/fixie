namespace Fixie.Tests.VisualStudio.TestAdapter
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Fixie.Runner;
    using Fixie.Runner.Contracts;
    using Fixie.VisualStudio.TestAdapter;
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

            var testCases = sink.Messages
                .Select(jsonMessage => Payload<Test>(jsonMessage, "TestDiscovery.TestFound"))
                .OrderBy(x => x.FullyQualifiedName)
                .Select(x => x.ToVisualStudioType(assemblyPath))
                .ToArray();

            foreach (var testCase in testCases)
            {
                testCase.LocalExtensionData.ShouldBeNull();
                testCase.Id.ShouldNotEqual(Guid.Empty);
                testCase.ExecutorUri.ShouldEqual(VsTestExecutor.Uri);
                testCase.Source.ShouldEqual(assemblyPath);
                testCase.DisplayName.ShouldEqual(testCase.FullyQualifiedName);

                testCase.CodeFilePath.EndsWith("MessagingTests.cs").ShouldBeTrue();
                testCase.LineNumber.ShouldBeGreaterThan(0);
            }

            testCases.Select(x => x.FullyQualifiedName)
                .ShouldEqual(
                    TestClass + ".Fail",
                    TestClass + ".FailByAssertion",
                    TestClass + ".Pass",
                    TestClass + ".SkipWithoutReason",
                    TestClass + ".SkipWithReason");
        }

        public void ShouldDefaultSourceLocationPropertiesWhenSourceInspectionThrows()
        {
            const string invalidAssemblyPath = "assembly.path.dll";

            var sink = new StubDesignTimeSink();
            Discover(new DesignTimeDiscoveryListener(sink, invalidAssemblyPath));

            sink.LogEntries.Count.ShouldEqual(5);

            var testCases = sink.Messages
                .Select(jsonMessage => Payload<Test>(jsonMessage, "TestDiscovery.TestFound"))
                .OrderBy(x => x.FullyQualifiedName)
                .Select(x => x.ToVisualStudioType(invalidAssemblyPath))
                .ToArray();

            foreach (var testCase in testCases)
            {
                testCase.LocalExtensionData.ShouldBeNull();
                testCase.Id.ShouldNotEqual(Guid.Empty);
                testCase.ExecutorUri.ShouldEqual(VsTestExecutor.Uri);
                testCase.Source.ShouldEqual(invalidAssemblyPath);
                testCase.DisplayName.ShouldEqual(testCase.FullyQualifiedName);

                testCase.CodeFilePath.ShouldBeNull();
                testCase.LineNumber.ShouldEqual(-1);
            }

            testCases.Select(x => x.FullyQualifiedName)
                .ShouldEqual(
                    TestClass + ".Fail",
                    TestClass + ".FailByAssertion",
                    TestClass + ".Pass",
                    TestClass + ".SkipWithoutReason",
                    TestClass + ".SkipWithReason");
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