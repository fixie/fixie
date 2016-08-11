namespace Fixie.Tests.VisualStudio.TestAdapter
{
    using System;
    using System.Collections.Generic;
    using Fixie.Internal;
    using Fixie.Runner;
    using Fixie.Runner.Contracts;
    using Newtonsoft.Json;
    using Should;

    using DotNetTest = Fixie.Runner.Contracts.Test;
    using DotNetTestResult = Fixie.Runner.Contracts.TestResult;

    using VsTestOutcome = Microsoft.VisualStudio.TestPlatform.ObjectModel.TestOutcome;
    using VsTestCase = Microsoft.VisualStudio.TestPlatform.ObjectModel.TestCase;
    using VsTestResult = Microsoft.VisualStudio.TestPlatform.ObjectModel.TestResult;
    using VsTestResultMessage = Microsoft.VisualStudio.TestPlatform.ObjectModel.TestResultMessage;

    using Fixie.VisualStudio.TestAdapter;
    using System.Linq;

    public class VisualStudioExecutionMappingTests : MessagingTests
    {
        public void ShouldMapTestResultContractsToVisualStudioTypes()
        {
            const string assemblyPath = "assembly.path.dll";

            var sink = new StubDesignTimeSink();
            var listener = new DesignTimeExecutionListener(sink);

            using (var console = new RedirectedConsole())
            {
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

            var starts = new List<VsTestCase>();
            var results = new List<VsTestResult>();

            starts.Add(Payload<DotNetTest>(sink.Messages[0], "TestExecution.TestStarted").ToVisualStudioType(assemblyPath));
            results.Add(Payload<DotNetTestResult>(sink.Messages[1], "TestExecution.TestResult").ToVisualStudioType(assemblyPath));

            starts.Add(Payload<DotNetTest>(sink.Messages[2], "TestExecution.TestStarted").ToVisualStudioType(assemblyPath));
            results.Add(Payload<DotNetTestResult>(sink.Messages[3], "TestExecution.TestResult").ToVisualStudioType(assemblyPath));

            starts.Add(Payload<DotNetTest>(sink.Messages[4], "TestExecution.TestStarted").ToVisualStudioType(assemblyPath));
            results.Add(Payload<DotNetTestResult>(sink.Messages[5], "TestExecution.TestResult").ToVisualStudioType(assemblyPath));

            starts.Add(Payload<DotNetTest>(sink.Messages[6], "TestExecution.TestStarted").ToVisualStudioType(assemblyPath));
            results.Add(Payload<DotNetTestResult>(sink.Messages[7], "TestExecution.TestResult").ToVisualStudioType(assemblyPath));

            starts.Add(Payload<DotNetTest>(sink.Messages[8], "TestExecution.TestStarted").ToVisualStudioType(assemblyPath));
            results.Add(Payload<DotNetTestResult>(sink.Messages[9], "TestExecution.TestResult").ToVisualStudioType(assemblyPath));

            foreach (var start in starts)
            {
                start.Id.ShouldNotEqual(Guid.Empty);
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
                result.Traits.ShouldBeEmpty();
                result.Attachments.ShouldBeEmpty();
                result.ComputerName.ShouldEqual(Environment.MachineName);
                result.TestCase.Id.ShouldNotEqual(Guid.Empty);
                result.TestCase.Traits.ShouldBeEmpty();
                result.TestCase.LocalExtensionData.ShouldBeNull();
                result.TestCase.Source.ShouldEqual("assembly.path.dll");

                //Source locations are a discovery-time concern.
                result.TestCase.CodeFilePath.ShouldBeNull();
                result.TestCase.LineNumber.ShouldEqual(-1);
            }

            var skipWithReason = results[0];
            var skipWithoutReason = results[1];
            var fail = results[2];
            var failByAssertion = results[3];
            var pass = results[4];

            skipWithReason.TestCase.FullyQualifiedName.ShouldEqual(TestClass + ".SkipWithReason");
            skipWithReason.TestCase.DisplayName.ShouldEqual(TestClass + ".SkipWithReason");
            skipWithReason.TestCase.ExecutorUri.ToString().ShouldEqual("executor://fixie.visualstudio/");
            skipWithReason.Outcome.ShouldEqual(VsTestOutcome.Skipped);
            skipWithReason.ErrorMessage.ShouldEqual("Skipped with reason.");
            skipWithReason.ErrorStackTrace.ShouldBeNull();
            skipWithReason.DisplayName.ShouldEqual(TestClass + ".SkipWithReason");
            skipWithReason.Messages.ShouldBeEmpty();
            skipWithReason.Duration.ShouldEqual(TimeSpan.Zero);

            skipWithoutReason.TestCase.FullyQualifiedName.ShouldEqual(TestClass + ".SkipWithoutReason");
            skipWithoutReason.TestCase.DisplayName.ShouldEqual(TestClass + ".SkipWithoutReason");
            skipWithoutReason.TestCase.ExecutorUri.ToString().ShouldEqual("executor://fixie.visualstudio/");
            skipWithoutReason.Outcome.ShouldEqual(VsTestOutcome.Skipped);
            skipWithoutReason.ErrorMessage.ShouldBeNull();
            skipWithoutReason.ErrorStackTrace.ShouldBeNull();
            skipWithoutReason.DisplayName.ShouldEqual(TestClass + ".SkipWithoutReason");
            skipWithoutReason.Messages.ShouldBeEmpty();
            skipWithoutReason.Duration.ShouldEqual(TimeSpan.Zero);

            fail.TestCase.FullyQualifiedName.ShouldEqual(TestClass + ".Fail");
            fail.TestCase.DisplayName.ShouldEqual(TestClass + ".Fail");
            fail.TestCase.ExecutorUri.ToString().ShouldEqual("executor://fixie.visualstudio/");
            fail.Outcome.ShouldEqual(VsTestOutcome.Failed);
            fail.ErrorMessage.ShouldEqual("'Fail' failed!");
            fail.ErrorStackTrace
                .CleanStackTraceLineNumbers()
                .Lines()
                .ShouldEqual("Fixie.Tests.FailureException", At("Fail()"));
            fail.DisplayName.ShouldEqual(TestClass + ".Fail");
            fail.Messages.Count.ShouldEqual(1);
            fail.Messages[0].Category.ShouldEqual(VsTestResultMessage.StandardOutCategory);
            fail.Messages[0].Text.Lines().ShouldEqual("Console.Out: Fail", "Console.Error: Fail");
            fail.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);

            failByAssertion.TestCase.FullyQualifiedName.ShouldEqual(TestClass + ".FailByAssertion");
            failByAssertion.TestCase.DisplayName.ShouldEqual(TestClass + ".FailByAssertion");
            failByAssertion.TestCase.ExecutorUri.ToString().ShouldEqual("executor://fixie.visualstudio/");
            failByAssertion.Outcome.ShouldEqual(VsTestOutcome.Failed);
            failByAssertion.ErrorMessage.Lines().ShouldEqual(
                "Assert.Equal() Failure",
                "Expected: 2",
                "Actual:   1");
            failByAssertion.ErrorStackTrace
                .CleanStackTraceLineNumbers()
                .ShouldEqual(At("FailByAssertion()"));
            failByAssertion.DisplayName.ShouldEqual(TestClass + ".FailByAssertion");
            failByAssertion.Messages.Count.ShouldEqual(1);
            failByAssertion.Messages[0].Category.ShouldEqual(VsTestResultMessage.StandardOutCategory);
            failByAssertion.Messages[0].Text.Lines().ShouldEqual("Console.Out: FailByAssertion", "Console.Error: FailByAssertion");
            failByAssertion.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);

            pass.TestCase.FullyQualifiedName.ShouldEqual(TestClass + ".Pass");
            pass.TestCase.DisplayName.ShouldEqual(TestClass + ".Pass");
            pass.TestCase.ExecutorUri.ToString().ShouldEqual("executor://fixie.visualstudio/");
            pass.Outcome.ShouldEqual(VsTestOutcome.Passed);
            pass.ErrorMessage.ShouldBeNull();
            pass.ErrorStackTrace.ShouldBeNull();
            pass.DisplayName.ShouldEqual(TestClass + ".Pass");
            pass.Messages.Count.ShouldEqual(1);
            pass.Messages[0].Category.ShouldEqual(VsTestResultMessage.StandardOutCategory);
            pass.Messages[0].Text.Lines().ShouldEqual("Console.Out: Pass", "Console.Error: Pass");
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