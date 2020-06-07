namespace Fixie.Tests.TestAdapter
{
    using System;
    using System.Collections.Generic;
    using Fixie.TestAdapter;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
    using Assertions;
    using Fixie.Internal;
    using static System.Environment;
    using static Fixie.Internal.Serialization;

    public class ExecutionListenerTests
    {
        public void ShouldMapMessagesToVsTestExecutionRecorder()
        {
            const string assemblyPath = "assembly.path.dll";
            var recorder = new StubExecutionRecorder();

            var executionRecorder = new ExecutionListener(recorder, assemblyPath);

            var @case = Case("Pass", 1);
            executionRecorder.Handle(new CaseStarted(@case));
            @case.Duration = TimeSpan.FromMilliseconds(1000);
            @case.Output = "Output";
            executionRecorder.Handle(new CasePassed(@case));

            @case = Case("Fail");
            executionRecorder.Handle(new CaseStarted(@case));
            @case.Duration = TimeSpan.FromMilliseconds(2000);
            @case.Output = "Output";
            @case.Fail(new StubException("Exception Message"));
            executionRecorder.Handle(new CaseFailed(@case));

            @case = Case("Skip");
            executionRecorder.Handle(new CaseStarted(@case));
            @case.Skip("Skip Reason");
            executionRecorder.Handle(new CaseSkipped(@case));

            var className = typeof(SampleTestClass).FullName;

            var starts = recorder.TestStarts;
            starts.Count.ShouldBe(3);
            starts[0].ShouldBeExecutionTimeTest(className+".Pass", assemblyPath);
            starts[1].ShouldBeExecutionTimeTest(className+".Fail", assemblyPath);
            starts[2].ShouldBeExecutionTimeTest(className+".Skip", assemblyPath);

            var results = recorder.TestResults;
            results.Count.ShouldBe(3);

            foreach (var result in results)
            {
                result.Traits.ShouldBeEmpty();
                result.Attachments.ShouldBeEmpty();
                result.ComputerName.ShouldBe(MachineName);
            }

            var pass = results[0];
            var fail = results[1];
            var skip = results[2];

            pass.TestCase.ShouldBeExecutionTimeTest(className+".Pass", assemblyPath);
            pass.TestCase.DisplayName.ShouldBe(className+".Pass");
            pass.Outcome.ShouldBe(TestOutcome.Passed);
            pass.ErrorMessage.ShouldBe(null);
            pass.ErrorStackTrace.ShouldBe(null);
            pass.DisplayName.ShouldBe(className+".Pass(1)");
            pass.Messages.Count.ShouldBe(1);
            pass.Messages[0].Category.ShouldBe(TestResultMessage.StandardOutCategory);
            pass.Messages[0].Text.ShouldBe("Output");
            pass.Duration.ShouldBe(TimeSpan.FromMilliseconds(1000));

            fail.TestCase.ShouldBeExecutionTimeTest(className+".Fail", assemblyPath);
            fail.TestCase.DisplayName.ShouldBe(className+".Fail");
            fail.Outcome.ShouldBe(TestOutcome.Failed);
            fail.ErrorMessage.ShouldBe("Exception Message");
            fail.ErrorStackTrace.ShouldBe(typeof(StubException).FullName + NewLine + "Exception Stack Trace");
            fail.DisplayName.ShouldBe(className+".Fail");
            fail.Messages.Count.ShouldBe(1);
            fail.Messages[0].Category.ShouldBe(TestResultMessage.StandardOutCategory);
            fail.Messages[0].Text.ShouldBe("Output");
            fail.Duration.ShouldBe(TimeSpan.FromMilliseconds(2000));

            skip.TestCase.ShouldBeExecutionTimeTest(className+".Skip", assemblyPath);
            skip.TestCase.DisplayName.ShouldBe(className+".Skip");
            skip.Outcome.ShouldBe(TestOutcome.Skipped);
            skip.ErrorMessage.ShouldBe("Skip Reason");
            skip.ErrorStackTrace.ShouldBe(null);
            skip.DisplayName.ShouldBe(className+".Skip");
            skip.Messages.ShouldBeEmpty();
            skip.Duration.ShouldBe(TimeSpan.Zero);
        }

        static T Deserialized<T>(T original)
        {
            // Because the inter-process communication between the VsTest process
            // and the test assembly process is not exercised in these single-process
            // tests, put a given sample message through the same serialization round
            // trip that would be applied at runtime, in order to detect data loss.

            return Deserialize<T>(Serialize(original));
        }

        static Case Case(string methodName, params object?[] parameters)
            => new Case(typeof(SampleTestClass).GetInstanceMethod(methodName), parameters);

        class SampleTestClass
        {
            public void Pass(int x) { }
            public void Fail() { }
            public void Skip() { }
        }

        class StubException : Exception
        {
            public StubException(string message)
                : base(message) { }

            public override string StackTrace
                => "Exception Stack Trace";
        }

        class StubExecutionRecorder : ITestExecutionRecorder
        {
            public List<TestCase> TestStarts { get; } = new List<TestCase>();
            public List<TestResult> TestResults { get; } = new List<TestResult>();

            public void RecordResult(TestResult testResult)
                => TestResults.Add(testResult);

            public void SendMessage(TestMessageLevel testMessageLevel, string message)
                => throw new NotImplementedException();

            public void RecordStart(TestCase testCase)
                => TestStarts.Add(testCase);

            public void RecordEnd(TestCase testCase, TestOutcome outcome)
                => throw new NotImplementedException();

            public void RecordAttachments(IList<AttachmentSet> attachmentSets)
                => throw new NotImplementedException();
        }
    }
}