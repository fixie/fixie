using System;

namespace Fixie.TestAdapter;

public class TestProcessExitException : Exception
{
    const int StackOverflowExitCode = -1073741571;

    public TestProcessExitException(int? exitCode)
        : base(GetMessage(exitCode))
    {
    }

    static string GetMessage(int? exitCode)
    {
        const string exitedUnexpectedly = "The test assembly process exited unexpectedly";

        if (exitCode != null)
        {
            var withExitCode = $"with exit code {exitCode}";

            return exitCode == StackOverflowExitCode
                ? $"{exitedUnexpectedly} {withExitCode}, indicating the test threw a StackOverflowException."
                : $"{exitedUnexpectedly} {withExitCode}.";
        }

        return $"{exitedUnexpectedly}.";
    }
}