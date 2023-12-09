using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Fixie.Reports;
using static System.Environment;

namespace Fixie.Tests;

class GitHubReport :
    IHandler<ExecutionStarted>,
    IHandler<ExecutionCompleted>
{
    readonly TestEnvironment environment;

    public GitHubReport(TestEnvironment environment)
        => this.environment = environment;

    public async Task Handle(ExecutionStarted message)
    {
        var assembly = Path.GetFileNameWithoutExtension(environment.Assembly.Location);
        var framework = environment.TargetFramework;

        await AppendToJobSummary($"## {assembly} ({framework}){NewLine}{NewLine}");
    }

    public async Task Handle(ExecutionCompleted message)
    {
        string severity;
        string detail;

        if (message.Total == 0)
        {
            severity = "red";
            detail = "No tests found.";
        }
        else
        {
            severity = "green";

            var parts = new List<string>();

            if (message.Passed > 0)
                parts.Add($"{message.Passed} passed");

            if (message.Skipped > 0)
            {
                severity = "yellow";
                parts.Add($"{message.Skipped} skipped");
            }

            if (message.Failed > 0)
            {
                severity = "red";
                parts.Add($"{message.Failed} failed");
            }

            detail = string.Join(", ", parts);
        }

        await AppendToJobSummary($":{severity}_circle: {detail}{NewLine}{NewLine}");
    }

    static async Task AppendToJobSummary(string summary)
    {
        if (GetEnvironmentVariable("GITHUB_STEP_SUMMARY") is string summaryFile)
            await File.AppendAllTextAsync(summaryFile, summary);
    }
}