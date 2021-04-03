namespace Fixie.Tests
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Threading.Tasks;
    using Assertions;

    class PrimaryConvention : Execution
    {
        public async Task RunAsync(TestAssembly testAssembly)
        {
            int failures = 0;
            Exception? singleFailure = null;

            foreach (var test in testAssembly.Tests)
            {
                var result = await test.RunAsync();

                if (result is Failed failure)
                {
                    failures++;

                    singleFailure = failures == 1 ? failure.Reason : null;
                }
            }

            if (singleFailure is AssertException exception)
                if (!exception.HasCompactRepresentations)
                    LaunchDiffTool(exception);
        }

        static void LaunchDiffTool(AssertException exception)
        {
            var tempPath = Path.GetTempPath();
            var expectedPath = Path.Combine(tempPath, "expected.txt");
            var actualPath = Path.Combine(tempPath, "actual.txt");

            var diffCommand = $"code --diff \"{expectedPath}\" \"{actualPath}\"";

            File.WriteAllText(expectedPath, exception.Expected);
            File.WriteAllText(actualPath, exception.Actual);

            using (Process.Start("cmd", $"/c \"{diffCommand}\""))  {  }
        }
    }
}
