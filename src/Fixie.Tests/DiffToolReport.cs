using Fixie.Reports;
using DiffEngine;
using Fixie.Assertions;
namespace Fixie.Tests;

class DiffToolReport : IHandler<TestFailed>, IHandler<ExecutionCompleted>
{
    int failures;
    Exception? singleFailure;

    public Task Handle(TestFailed message)
    {
        failures++;

        singleFailure = failures == 1 ? message.Reason : null;

        return Task.CompletedTask;
    }

    public async Task Handle(ExecutionCompleted message)
    {
        if (singleFailure is AssertException exception)
            if (exception.HasMultilineRepresentation)
                await LaunchDiffTool(exception);
    }

    static async Task LaunchDiffTool(AssertException exception)
    {
        var tempPath = Path.GetTempPath();
        var expectedPath = Path.Combine(tempPath, "expected.txt");
        var actualPath = Path.Combine(tempPath, "actual.txt");

        File.WriteAllText(expectedPath, exception.Expected);
        File.WriteAllText(actualPath, exception.Actual);

        await DiffRunner.LaunchAsync(expectedPath, actualPath);
    }
}