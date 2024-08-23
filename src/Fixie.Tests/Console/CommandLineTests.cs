using Fixie.Console;

namespace Fixie.Tests.Console;

public class CommandLineTests
{
    public void ShouldPartitionRunnerArgumentsFromCustomArguments()
    {
        CommandLine.Partition([
            "Example.Tests", "--configuration", "Release", "--framework", "net8.0",
            "--",
            "customA", "customB", "customC"
        ], out var runnerArguments, out var customArguments);
        runnerArguments.ShouldBe(["Example.Tests", "--configuration", "Release", "--framework", "net8.0"]);
        customArguments.ShouldBe(["customA", "customB", "customC"]);

        CommandLine.Partition(["Example.Tests", "--", "custom"], out runnerArguments, out customArguments);
        runnerArguments.ShouldBe(["Example.Tests"]);
        customArguments.ShouldBe(["custom"]);

        //Characterization coverage of undesirable behavior.
        CommandLine.Partition(["Example.Tests", "--", "--", "customA", "--", "--", "customB"], out runnerArguments, out customArguments);
        runnerArguments.ShouldBe(["Example.Tests"]);
        customArguments.ShouldBe(["customA", "customB"]);

        CommandLine.Partition(["--", "custom"], out runnerArguments, out customArguments);
        runnerArguments.ShouldBe([]);
        customArguments.ShouldBe(["custom"]);

        CommandLine.Partition(["Example.Tests", "--"], out runnerArguments, out customArguments);
        runnerArguments.ShouldBe(["Example.Tests"]);
        customArguments.ShouldBe([]);

        CommandLine.Partition(["--"], out runnerArguments, out customArguments);
        runnerArguments.ShouldBe([]);
        customArguments.ShouldBe([]);

        CommandLine.Partition(["Example.Tests", "unexpectedCustom"], out runnerArguments, out customArguments);
        runnerArguments.ShouldBe(["Example.Tests", "unexpectedCustom"]);
        customArguments.ShouldBe([]);
    }
}