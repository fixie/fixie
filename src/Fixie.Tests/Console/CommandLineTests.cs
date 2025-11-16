using Fixie.Console;

namespace Fixie.Tests.Console;

public class CommandLineTests
{
    public void ShouldPartitionRunnerArgumentsFromCustomArguments()
    {
        CommandLine.Partition([
            "Example.Tests", "--configuration", "Release", "--framework", "net10.0",
            "--",
            "customA", "customB", "customC"
        ], out var runnerArguments, out var customArguments);
        runnerArguments.ShouldMatch(["Example.Tests", "--configuration", "Release", "--framework", "net10.0"]);
        customArguments.ShouldMatch(["customA", "customB", "customC"]);

        CommandLine.Partition(["Example.Tests", "--", "custom"], out runnerArguments, out customArguments);
        runnerArguments.ShouldMatch(["Example.Tests"]);
        customArguments.ShouldMatch(["custom"]);

        CommandLine.Partition(["Example.Tests", "--", "--", "customA", "--", "--", "customB"], out runnerArguments, out customArguments);
        runnerArguments.ShouldMatch(["Example.Tests"]);
        customArguments.ShouldMatch(["--", "customA", "--", "--", "customB"]);

        CommandLine.Partition(["--", "custom"], out runnerArguments, out customArguments);
        runnerArguments.ShouldMatch([]);
        customArguments.ShouldMatch(["custom"]);

        CommandLine.Partition(["Example.Tests", "--"], out runnerArguments, out customArguments);
        runnerArguments.ShouldMatch(["Example.Tests"]);
        customArguments.ShouldMatch([]);

        CommandLine.Partition(["--"], out runnerArguments, out customArguments);
        runnerArguments.ShouldMatch([]);
        customArguments.ShouldMatch([]);

        CommandLine.Partition([], out runnerArguments, out customArguments);
        runnerArguments.ShouldMatch([]);
        customArguments.ShouldMatch([]);

        CommandLine.Partition(["Example.Tests"], out runnerArguments, out customArguments);
        runnerArguments.ShouldMatch(["Example.Tests"]);
        customArguments.ShouldMatch([]);

        CommandLine.Partition(["Example.Tests", "unexpectedCustom"], out runnerArguments, out customArguments);
        runnerArguments.ShouldMatch(["Example.Tests", "unexpectedCustom"]);
        customArguments.ShouldMatch([]);
    }
}