﻿using Fixie.Internal;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using static System.Environment;

namespace Fixie.TestAdapter;

using TestResult = Microsoft.VisualStudio.TestPlatform.ObjectModel.TestResult;

class VsExecutionRecorder
{
    readonly ITestExecutionRecorder log;
    readonly string assemblyPath;

    public VsExecutionRecorder(ITestExecutionRecorder log, string assemblyPath)
    {
        this.log = log;
        this.assemblyPath = assemblyPath;
    }

    public void Record(PipeMessage.TestStarted message)
    {
        var testCase = ToVsTestCase(message.Test);

        log.RecordStart(testCase);
    }

    public void Record(PipeMessage.TestSkipped result)
    {
        Record(result, x =>
        {
            x.Outcome = TestOutcome.Skipped;
            x.ErrorMessage = result.Reason;
        });
    }

    public void Record(PipeMessage.TestPassed result)
    {
        Record(result, x =>
        {
            x.Outcome = TestOutcome.Passed;
        });
    }

    public void Record(PipeMessage.TestFailed result)
    {
        Record(result, x =>
        {
            x.Outcome = TestOutcome.Failed;
            x.ErrorMessage = result.Reason.Message;
            x.ErrorStackTrace = result.Reason.Type +
                                NewLine +
                                result.Reason.StackTrace;
        });
    }

    void Record(PipeMessage.TestCompleted result, Action<TestResult> customize)
    {
        var testCase = ToVsTestCase(result.Test);

        var testResult = new TestResult(testCase)
        {
            DisplayName = result.TestCase,
            Duration = TimeSpan.FromMilliseconds(result.DurationInMilliseconds),
            ComputerName = MachineName
        };

        customize(testResult);

        log.RecordResult(testResult);
    }

    TestCase ToVsTestCase(string test)
    {
        return new TestCase(test, VsTestExecutor.Uri, assemblyPath);
    }
}