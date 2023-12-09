using System.Collections.Generic;
using System.Threading.Tasks;
using Fixie.Reports;

namespace Fixie.Tests;

public class StubReport :
    IHandler<TestDiscovered>,
    IHandler<TestSkipped>,
    IHandler<TestPassed>,
    IHandler<TestFailed>
{
    readonly List<string> log = new List<string>();

    public Task Handle(TestDiscovered message)
    {
        log.Add($"{message.Test} discovered");
        return Task.CompletedTask;
    }

    public Task Handle(TestSkipped message)
    {
        log.Add($"{message.TestCase} skipped: {message.Reason}");
        return Task.CompletedTask;
    }

    public Task Handle(TestPassed message)
    {
        log.Add($"{message.TestCase} passed");
        return Task.CompletedTask;
    }

    public Task Handle(TestFailed message)
    {
        log.Add($"{message.TestCase} failed: {message.Reason.Message}");
        return Task.CompletedTask;
    }

    public IEnumerable<string> Entries => log;
}