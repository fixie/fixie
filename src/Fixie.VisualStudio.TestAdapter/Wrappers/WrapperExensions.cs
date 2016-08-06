namespace Fixie.VisualStudio.TestAdapter.Wrappers
{
    using System;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;

    public static class WrapperExensions
    {
        public static TestCase ToVisualStudioType(this TestCaseModel message)
        {
            return new TestCase(message.MethodGroup, VsTestExecutor.Uri, message.AssemblyPath)
            {
                CodeFilePath = message.CodeFilePath,
                LineNumber = message.LineNumber ?? -1
            };
        }

        public static TestResult ToVisualStudioType(this TestResultModel message)
        {
            var testResult = new TestResult(message.TestCase.ToVisualStudioType())
            {
                DisplayName = message.Name,
                Outcome = (TestOutcome)Enum.Parse(typeof(TestOutcome), message.Status),
                Duration = message.Duration,
                ComputerName = Environment.MachineName,
                ErrorMessage = message.ErrorMessage,
                ErrorStackTrace = message.ErrorStackTrace
            };

            AttachCapturedConsoleOutput(message.Output, testResult);

            return testResult;
        }

        static void AttachCapturedConsoleOutput(string output, TestResult testResult)
        {
            if (!String.IsNullOrEmpty(output))
                testResult.Messages.Add(new TestResultMessage(TestResultMessage.StandardOutCategory, output));
        }
    }
}