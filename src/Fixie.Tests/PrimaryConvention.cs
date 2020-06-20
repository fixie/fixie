namespace Fixie.Tests
{
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using Assertions;
    using static System.Environment;
    using static Fixie.Internal.Maybe;

    class PrimaryConvention : Execution
    {
        public void Execute(TestClass testClass)
        {
            testClass.RunCases(@case =>
            {
                var instance = testClass.Construct();

                @case.Execute(instance);

                instance.Dispose();

                var methodWasExplicitlyRequested = testClass.TargetMethod != null;

                if (methodWasExplicitlyRequested && @case.Exception is AssertException exception)
                    LaunchDiffTool(exception);
            });
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
