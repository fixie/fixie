namespace Fixie.Tests
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Assertions;
    using Fixie.Reports;
    using static System.Environment;
    using static Fixie.Internal.Maybe;

    public class DiffToolReport : IHandler<TestFailed>, IHandler<ExecutionCompleted>
    {
        int failures;
        Exception? singleFailure;

        public Task Handle(TestFailed message)
        {
            failures++;

            singleFailure = failures == 1 ? message.Reason : null;

            return Task.CompletedTask;
        }

        public Task Handle(ExecutionCompleted message)
        {
            if (singleFailure is AssertException exception)
                if (!exception.HasCompactRepresentations)
                    LaunchDiffTool(exception);

            return Task.CompletedTask;
        }

        static void LaunchDiffTool(AssertException exception)
        {
            var tempPath = Path.GetTempPath();
            var expectedPath = Path.Combine(tempPath, "expected.txt");
            var actualPath = Path.Combine(tempPath, "actual.txt");

            if (Try(() => DiffCommand(expectedPath, actualPath), out var diffCommand))
            {
                File.WriteAllText(expectedPath, exception.Expected);
                File.WriteAllText(actualPath, exception.Actual);

                using (Process.Start("cmd", $"/c \"{diffCommand}\""))  {  }
            }
        }

        static string? DiffCommand(string expectedPath, string actualPath)
        {
            var gitconfig = Path.Combine(GetFolderPath(SpecialFolder.UserProfile), ".gitconfig");

            if (!File.Exists(gitconfig))
                return null;

            return File.ReadAllLines(gitconfig)
                .SkipWhile(x => !x.StartsWith("[difftool "))
                .Skip(1)
                .TakeWhile(x => !x.StartsWith("["))
                .Select(x => x.Split(new[] {'='}, 2))
                .Where(x => x[0].Trim() == "cmd")
                .Select(x => x[1].Trim()
                    .Replace("\\\"", "\"")
                    .Replace("$LOCAL", expectedPath)
                    .Replace("$REMOTE", actualPath))
                .SingleOrDefault();
        }
    }
}