namespace Fixie.Tests.Runner
{
    using System;
    using System.Collections.Generic;
    using Fixie.Internal;
    using Fixie.Runner;
    using Fixie.Runner.Contracts;
    using Newtonsoft.Json;
    using Should;
    using System.Linq;

    public class DesignTimeExecutionListenerTests : MessagingTests
    {
        public void ShouldReportResultsToExecutionRecorder()
        {
            var sink = new StubDesignTimeSink();

            using (var console = new RedirectedConsole())
            {
                var listener = new DesignTimeExecutionListener(sink);

                Run(listener);

                console.Lines()
                    .ShouldEqual(
                        "Console.Out: Fail",
                        "Console.Error: Fail",
                        "Console.Out: FailByAssertion",
                        "Console.Error: FailByAssertion",
                        "Console.Out: Pass",
                        "Console.Error: Pass");
            }

            sink.LogEntries.ShouldBeEmpty();
            sink.Messages.Count.ShouldEqual(10);

            var starts = new List<Test>();
            var results = new List<TestResult>();

            starts.Add(Payload<Test>(sink.Messages[0], "TestExecution.TestStarted"));
            results.Add(Payload<TestResult>(sink.Messages[1], "TestExecution.TestResult"));

            starts.Add(Payload<Test>(sink.Messages[2], "TestExecution.TestStarted"));
            results.Add(Payload<TestResult>(sink.Messages[3], "TestExecution.TestResult"));

            starts.Add(Payload<Test>(sink.Messages[4], "TestExecution.TestStarted"));
            results.Add(Payload<TestResult>(sink.Messages[5], "TestExecution.TestResult"));

            starts.Add(Payload<Test>(sink.Messages[6], "TestExecution.TestStarted"));
            results.Add(Payload<TestResult>(sink.Messages[7], "TestExecution.TestResult"));

            starts.Add(Payload<Test>(sink.Messages[8], "TestExecution.TestStarted"));
            results.Add(Payload<TestResult>(sink.Messages[9], "TestExecution.TestResult"));

            foreach (var start in starts)
            {
                start.Id.ShouldEqual(null);
                start.Properties.ShouldBeEmpty();

                //Source locations are a discovery-time concern.
                start.CodeFilePath.ShouldBeNull();
                start.LineNumber.ShouldBeNull();
            }

            starts.Select(x => x.FullyQualifiedName)
                .ShouldEqual(
                    TestClass + ".SkipWithReason",
                    TestClass + ".SkipWithoutReason",
                    TestClass + ".Fail",
                    TestClass + ".FailByAssertion",
                    TestClass + ".Pass");

            starts.Select(x => x.DisplayName)
                .ShouldEqual(
                    TestClass + ".SkipWithReason",
                    TestClass + ".SkipWithoutReason",
                    TestClass + ".Fail",
                    TestClass + ".FailByAssertion",
                    TestClass + ".Pass");

            foreach (var result in results)
            {
                result.ComputerName.ShouldEqual(Environment.MachineName);
                result.Test.Id.ShouldEqual(null);
                result.Test.Properties.ShouldBeEmpty();

                //Source locations are a discovery-time concern.
                result.Test.CodeFilePath.ShouldBeNull();
                result.Test.LineNumber.ShouldBeNull();
            }

            var skipWithReason = results[0];
            var skipWithoutReason = results[1];
            var fail = results[2];
            var failByAssertion = results[3];
            var pass = results[4];

            skipWithReason.Test.FullyQualifiedName.ShouldEqual(TestClass + ".SkipWithReason");
            skipWithReason.Test.DisplayName.ShouldEqual(TestClass + ".SkipWithReason");
            skipWithReason.Outcome.ShouldEqual(TestOutcome.Skipped);
            skipWithReason.ErrorMessage.ShouldEqual("Skipped with reason.");
            skipWithReason.ErrorStackTrace.ShouldBeNull();
            skipWithReason.DisplayName.ShouldEqual(TestClass + ".SkipWithReason");
            skipWithReason.Messages.ShouldBeEmpty();
            skipWithReason.Duration.ShouldEqual(TimeSpan.Zero);

            skipWithoutReason.Test.FullyQualifiedName.ShouldEqual(TestClass + ".SkipWithoutReason");
            skipWithoutReason.Test.DisplayName.ShouldEqual(TestClass + ".SkipWithoutReason");
            skipWithoutReason.Outcome.ShouldEqual(TestOutcome.Skipped);
            skipWithoutReason.ErrorMessage.ShouldBeNull();
            skipWithoutReason.ErrorStackTrace.ShouldBeNull();
            skipWithoutReason.DisplayName.ShouldEqual(TestClass + ".SkipWithoutReason");
            skipWithoutReason.Messages.ShouldBeEmpty();
            skipWithoutReason.Duration.ShouldEqual(TimeSpan.Zero);

            fail.Test.FullyQualifiedName.ShouldEqual(TestClass + ".Fail");
            fail.Test.DisplayName.ShouldEqual(TestClass + ".Fail");
            fail.Outcome.ShouldEqual(TestOutcome.Failed);
            fail.ErrorMessage.ShouldEqual("'Fail' failed!");
            fail.ErrorStackTrace
                .CleanStackTraceLineNumbers()
                .Lines()
                .ShouldEqual("Fixie.Tests.FailureException", At("Fail()"));
            fail.DisplayName.ShouldEqual(TestClass + ".Fail");
            fail.Messages.Single().Lines().ShouldEqual("Console.Out: Fail", "Console.Error: Fail");
            fail.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);

            failByAssertion.Test.FullyQualifiedName.ShouldEqual(TestClass + ".FailByAssertion");
            failByAssertion.Test.DisplayName.ShouldEqual(TestClass + ".FailByAssertion");
            failByAssertion.Outcome.ShouldEqual(TestOutcome.Failed);
            failByAssertion.ErrorMessage.Lines().ShouldEqual(
                "Assert.Equal() Failure",
                "Expected: 2",
                "Actual:   1");
            failByAssertion.ErrorStackTrace
                .CleanStackTraceLineNumbers()
                .ShouldEqual(At("FailByAssertion()"));
            failByAssertion.DisplayName.ShouldEqual(TestClass + ".FailByAssertion");
            failByAssertion.Messages.Single().Lines().ShouldEqual("Console.Out: FailByAssertion", "Console.Error: FailByAssertion");
            failByAssertion.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);

            pass.Test.FullyQualifiedName.ShouldEqual(TestClass + ".Pass");
            pass.Test.DisplayName.ShouldEqual(TestClass + ".Pass");
            pass.Outcome.ShouldEqual(TestOutcome.Passed);
            pass.ErrorMessage.ShouldBeNull();
            pass.ErrorStackTrace.ShouldBeNull();
            pass.DisplayName.ShouldEqual(TestClass + ".Pass");
            pass.Messages.Single().Lines().ShouldEqual("Console.Out: Pass", "Console.Error: Pass");
            pass.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);            
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