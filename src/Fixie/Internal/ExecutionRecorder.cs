using System.Threading.Channels;
using Fixie.Reports;

namespace Fixie.Internal;

class ExecutionRecorder(ChannelWriter<IMessage> channelWriter)
{
    public async Task Start(Test test)
    {
        var message = new TestStarted(test);
        await channelWriter.WriteAsync(message);
    }

    public async Task Skip(Test test, string name, string reason, TimeSpan duration)
    {
        var message = new TestSkipped(test.Name, name, duration, reason);
        await channelWriter.WriteAsync(message);
    }

    public async Task Pass(Test test, string name, TimeSpan duration)
    {
        var message = new TestPassed(test.Name, name, duration);
        await channelWriter.WriteAsync(message);
    }

    public async Task Fail(Test test, string name, Exception reason, TimeSpan duration)
    {
        var message = new TestFailed(test.Name, name, duration, reason);
        await channelWriter.WriteAsync(message);
    }
}