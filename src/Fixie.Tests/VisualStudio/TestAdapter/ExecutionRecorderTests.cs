namespace Fixie.Tests.VisualStudio.TestAdapter
{
    using System;
    using System.Collections.Generic;
    using Fixie.VisualStudio.TestAdapter;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
    using Assertions;
    using Fixie.Internal.Listeners;
    using static System.Environment;

    public class ExecutionRecorderTests
    {
        public void ShouldMapResultsToVisualStudioExecutionRecorder()
        {
            const string assemblyPath = "assembly.path.dll";
            var recorder = new StubExecutionRecorder();

            var executionRecorder = new ExecutionRecorder(recorder, assemblyPath);

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

            var results = recorder.TestResults;
            results.Count.ShouldEqual(3);

            foreach (var result in results)
            {
                result.Traits.ShouldBeEmpty();
                result.Attachments.ShouldBeEmpty();
                result.ComputerName.ShouldEqual(MachineName);
            }

            var pass = results[0];
            var fail = results[1];
            var skip = results[2];

            pass.TestCase.ShouldBeExecutionTimeTest("Namespace.Class.Pass", assemblyPath);
            pass.TestCase.DisplayName.ShouldEqual("Namespace.Class.Pass");
            pass.Outcome.ShouldEqual(TestOutcome.Passed);
            pass.ErrorMessage.ShouldBeNull();
            pass.ErrorStackTrace.ShouldBeNull();
            pass.DisplayName.ShouldEqual("Namespace.Class.Pass(1)");
            pass.Messages.Count.ShouldEqual(1);
            pass.Messages[0].Category.ShouldEqual(TestResultMessage.StandardOutCategory);
            pass.Messages[0].Text.ShouldEqual("Output");
            pass.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);

            fail.TestCase.ShouldBeExecutionTimeTest("Namespace.Class.Fail", assemblyPath);
            fail.TestCase.DisplayName.ShouldEqual("Namespace.Class.Fail");
            fail.Outcome.ShouldEqual(TestOutcome.Failed);
            fail.ErrorMessage.ShouldEqual("Exception Message");
            fail.ErrorStackTrace.ShouldEqual("Exception Type" + NewLine + "Exception Stack Trace");
            fail.DisplayName.ShouldEqual("Namespace.Class.Fail");
            fail.Messages.Count.ShouldEqual(1);
            fail.Messages[0].Category.ShouldEqual(TestResultMessage.StandardOutCategory);
            fail.Messages[0].Text.ShouldEqual("Output");
            fail.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);

            skip.TestCase.ShouldBeExecutionTimeTest("Namespace.Class.Skip", assemblyPath);
            skip.TestCase.DisplayName.ShouldEqual("Namespace.Class.Skip");
            skip.Outcome.ShouldEqual(TestOutcome.Skipped);
            skip.ErrorMessage.ShouldEqual("Skip Reason");
            skip.ErrorStackTrace.ShouldBeNull();
            skip.DisplayName.ShouldEqual("Namespace.Class.Skip");
            skip.Messages.ShouldBeEmpty();
            skip.Duration.ShouldEqual(TimeSpan.Zero);
        }

        class StubExecutionRecorder : ITestExecutionRecorder
        {
            public List<TestResult> TestResults { get; } = new List<TestResult>();

            public void RecordResult(TestResult testResult)
                => TestResults.Add(testResult);

            public void SendMessage(TestMessageLevel testMessageLevel, string message)
                => throw new NotImplementedException();

            public void RecordStart(TestCase testCase)
                => throw new NotImplementedException();

            public void RecordEnd(TestCase testCase, TestOutcome outcome)
                => throw new NotImplementedException();

            public void RecordAttachments(IList<AttachmentSet> attachmentSets)
                => throw new NotImplementedException();
        }
    }
}