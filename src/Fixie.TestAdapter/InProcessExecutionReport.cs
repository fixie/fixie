namespace Fixie.TestAdapter;

using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Reports;
using static System.Environment;

class InProcessExecutionReport :
    IHandler<TestStarted>,
    IHandler<TestSkipped>,
    IHandler<TestPassed>,
    IHandler<TestFailed>
{
    readonly ITestExecutionRecorder log;
    readonly string assemblyPath;

    public InProcessExecutionReport(ITestExecutionRecorder log, string assemblyPath)
    {
        this.log = log;
        this.assemblyPath = assemblyPath;
    }

    public Task Handle(TestStarted message)
    {
        var testCase = ToVsTestCase(message.Test);

        log.RecordStart(testCase);
            
        return Task.CompletedTask;
    }

    public Task Handle(TestSkipped message)
    {
        Record(message, x =>
        {
            x.Outcome = TestOutcome.Skipped;
            x.ErrorMessage = message.Reason;
        });
            
        return Task.CompletedTask;
    }

    public Task Handle(TestPassed message)
    {
        Record(message, x =>
        {
            x.Outcome = TestOutcome.Passed;
        });

        return Task.CompletedTask;
    }

    public Task Handle(TestFailed message)
    {
        Record(message, x =>
        {
            x.Outcome = TestOutcome.Failed;
            x.ErrorMessage = message.Reason.Message;
            x.ErrorStackTrace = message.Reason.GetType().FullName +
                                NewLine +
                                message.Reason.LiterateStackTrace();
        });
            
        return Task.CompletedTask;
    }

    void Record(TestCompleted result, Action<TestResult> customize)
    {
        var testCase = ToVsTestCase(result.Test);

        var testResult = new TestResult(testCase)
        {
            DisplayName = result.TestCase,
            Duration = result.Duration,
            ComputerName = MachineName
        };

        customize(testResult);

        AttachCapturedConsoleOutput(result.Output, testResult);

        log.RecordResult(testResult);
    }

    TestCase ToVsTestCase(string fullyQualifiedName)
    {
        return new TestCase(fullyQualifiedName, VsTestExecutor.Uri, assemblyPath);
    }

    static void AttachCapturedConsoleOutput(string output, TestResult testResult)
    {
        if (!string.IsNullOrEmpty(output))
            testResult.Messages.Add(new TestResultMessage(TestResultMessage.StandardOutCategory, output));
    }
}