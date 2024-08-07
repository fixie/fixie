using System.Diagnostics;
using System.Threading.Channels;
using Fixie.Reports;

namespace Fixie.Internal;

class ExecutionRecorder
{
    readonly ChannelWriter<IMessage> channelWriter;

    readonly Stopwatch caseStopwatch;

    public ExecutionRecorder(ChannelWriter<IMessage> channelWriter)
    {
        this.channelWriter = channelWriter;

        caseStopwatch = new Stopwatch();
        caseStopwatch.Restart();
    }

    public async Task Start(Test test)
    {
        var message = new TestStarted(test);
        await channelWriter.WriteAsync(message);
    }

    public async Task Skip(Test test, string name, string reason)
    {
        var duration = caseStopwatch.Elapsed;

        var message = new TestSkipped(test.Name, name, duration, reason);
        await channelWriter.WriteAsync(message);

        caseStopwatch.Restart();
    }

    public async Task Pass(Test test, string name)
    {
        var duration = caseStopwatch.Elapsed;

        var message = new TestPassed(test.Name, name, duration);
        await channelWriter.WriteAsync(message);

        caseStopwatch.Restart();
    }

    public async Task Fail(Test test, string name, Exception reason)
    {
        var duration = caseStopwatch.Elapsed;

        var message = new TestFailed(test.Name, name, duration, reason);
        await channelWriter.WriteAsync(message);

        caseStopwatch.Restart();
    }
}