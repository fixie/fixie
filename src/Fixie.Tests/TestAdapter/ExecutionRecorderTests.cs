namespace Fixie.Tests.TestAdapter
{
    using System;
    using System.Collections.Generic;
    using Fixie.TestAdapter;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
    using Assertions;
    using Fixie.Internal.Listeners;
    using static System.Environment;

    public class ExecutionRecorderTests
    {
        public void ShouldMapMessagesToVsTestExecutionRecorder()
        {
            const string assemblyPath = "assembly.path.dll";
            var recorder = new StubExecutionRecorder();

            var executionRecorder = new ExecutionRecorder(recorder, assemblyPath);

            executionRecorder.Record(new PipeMessage.CaseStarted
            {
                Test = new PipeMessage.Test
                {
                    Class = "Namespace.Class",
                    Method = "Pass",
                    Name = "Namespace.Class.Pass",
                },
                Name = "Namespace.Class.Pass(1)"
            });

            executionRecorder.Record(new PipeMessage.CasePassed
            {
                Test = new PipeMessage.Test
                {
                    Class = "Namespace.Class",
                    Method = "Pass",
                    Name = "Namespace.Class.Pass",
                },
                Name = "Namespace.Class.Pass(1)",
                Duration = TimeSpan.FromSeconds(1),
                Output = "Output"
            });

            executionRecorder.Record(new PipeMessage.CaseStarted
            {
                Test = new PipeMessage.Test
                {
                    Class = "Namespace.Class",
                    Method = "Fail",
                    Name = "Namespace.Class.Fail",
                },
                Name = "Namespace.Class.Fail"
            });

            executionRecorder.Record(new PipeMessage.CaseFailed
            {
                Test = new PipeMessage.Test
                {
                    Class = "Namespace.Class",
                    Method = "Fail",
                    Name = "Namespace.Class.Fail",
                },
                Name = "Namespace.Class.Fail",
                Duration = TimeSpan.FromSeconds(2),
                Output = "Output",
                Exception = new PipeMessage.Exception
                {
                    Type = "Exception Type",
                    Message = "Exception Message",
                    StackTrace = "Exception Stack Trace"
                }
            });

            executionRecorder.Record(new PipeMessage.CaseStarted
            {
                Test = new PipeMessage.Test
                {
                    Class = "Namespace.Class",
                    Method = "Skip",
                    Name = "Namespace.Class.Skip",
                },
                Name = "Namespace.Class.Skip"
            });

            executionRecorder.Record(new PipeMessage.CaseSkipped
            {
                Test = new PipeMessage.Test
                {
                    Class = "Namespace.Class",
                    Method = "Skip",
                    Name = "Namespace.Class.Skip",
                },
                Name = "Namespace.Class.Skip",
                Duration = TimeSpan.Zero,
                Output = null,
                Reason = "Skip Reason"
            });

            var starts = recorder.TestStarts;
            starts.Count.ShouldBe(3);
            starts[0].ShouldBeExecutionTimeTest("Namespace.Class.Pass", assemblyPath);
            starts[1].ShouldBeExecutionTimeTest("Namespace.Class.Fail", assemblyPath);
            starts[2].ShouldBeExecutionTimeTest("Namespace.Class.Skip", assemblyPath);

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

            pass.TestCase.ShouldBeExecutionTimeTest("Namespace.Class.Pass", assemblyPath);
            pass.TestCase.DisplayName.ShouldBe("Namespace.Class.Pass");
            pass.Outcome.ShouldBe(TestOutcome.Passed);
            pass.ErrorMessage.ShouldBe(null);
            pass.ErrorStackTrace.ShouldBe(null);
            pass.DisplayName.ShouldBe("Namespace.Class.Pass(1)");
            pass.Messages.Count.ShouldBe(1);
            pass.Messages[0].Category.ShouldBe(TestResultMessage.StandardOutCategory);
            pass.Messages[0].Text.ShouldBe("Output");
            pass.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);

            fail.TestCase.ShouldBeExecutionTimeTest("Namespace.Class.Fail", assemblyPath);
            fail.TestCase.DisplayName.ShouldBe("Namespace.Class.Fail");
            fail.Outcome.ShouldBe(TestOutcome.Failed);
            fail.ErrorMessage.ShouldBe("Exception Message");
            fail.ErrorStackTrace.ShouldBe("Exception Type" + NewLine + "Exception Stack Trace");
            fail.DisplayName.ShouldBe("Namespace.Class.Fail");
            fail.Messages.Count.ShouldBe(1);
            fail.Messages[0].Category.ShouldBe(TestResultMessage.StandardOutCategory);
            fail.Messages[0].Text.ShouldBe("Output");
            fail.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);

            skip.TestCase.ShouldBeExecutionTimeTest("Namespace.Class.Skip", assemblyPath);
            skip.TestCase.DisplayName.ShouldBe("Namespace.Class.Skip");
            skip.Outcome.ShouldBe(TestOutcome.Skipped);
            skip.ErrorMessage.ShouldBe("Skip Reason");
            skip.ErrorStackTrace.ShouldBe(null);
            skip.DisplayName.ShouldBe("Namespace.Class.Skip");
            skip.Messages.ShouldBeEmpty();
            skip.Duration.ShouldBe(TimeSpan.Zero);
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