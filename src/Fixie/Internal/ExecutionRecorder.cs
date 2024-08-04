using System.Diagnostics;
using Fixie.Reports;

namespace Fixie.Internal;

class ExecutionRecorder
{
    readonly Bus bus;

    readonly ExecutionSummary assemblySummary;
    readonly Stopwatch assemblyStopwatch;
    readonly Stopwatch caseStopwatch;

    public ExecutionRecorder(Bus bus)
    {
        this.bus = bus;

        assemblySummary = new ExecutionSummary();

        assemblyStopwatch = new Stopwatch();
        caseStopwatch = new Stopwatch();
    }

    public async Task StartExecution()
    {
        await bus.Publish(new ExecutionStarted());
        assemblyStopwatch.Restart();
        caseStopwatch.Restart();
    }

    public async Task Start(Test test)
    {
        await bus.Publish(new TestStarted(test));
    }

    public async Task Skip(Test test, string name, string reason)
    {
        var duration = caseStopwatch.Elapsed;

        var message = new TestSkipped(test.Name, name, duration, reason);
        assemblySummary.Add(message);
        await bus.Publish(message);

        caseStopwatch.Restart();
    }

    public async Task Pass(Test test, string name)
    {
        var duration = caseStopwatch.Elapsed;

        var message = new TestPassed(test.Name, name, duration);
        assemblySummary.Add(message);
        await bus.Publish(message);

        caseStopwatch.Restart();
    }

    public async Task Fail(Test test, string name, Exception reason)
    {
        var duration = caseStopwatch.Elapsed;

        var message = new TestFailed(test.Name, name, duration, reason);
        assemblySummary.Add(message);
        await bus.Publish(message);

        caseStopwatch.Restart();
    }

    public async Task<ExecutionSummary> CompleteExecution()
    {
        var duration = assemblyStopwatch.Elapsed;
        caseStopwatch.Stop();
        assemblyStopwatch.Stop();

        await bus.Publish(new ExecutionCompleted(assemblySummary, duration));

        return assemblySummary;
    }
}