namespace Fixie.VisualStudio.TestAdapter
{
    using System;

    using DotNetTest = Runner.Contracts.Test;
    using DotNetTestResult = Runner.Contracts.TestResult;

    using VsTestOutcome = Microsoft.VisualStudio.TestPlatform.ObjectModel.TestOutcome;
    using VsTestCase = Microsoft.VisualStudio.TestPlatform.ObjectModel.TestCase;
    using VsTestResult = Microsoft.VisualStudio.TestPlatform.ObjectModel.TestResult;
    using VsTestResultMessage = Microsoft.VisualStudio.TestPlatform.ObjectModel.TestResultMessage;

    public static class MappingExtensions
    {
        public static VsTestCase ToVisualStudioType(this DotNetTest dotNetTest, string assemblyPath)
        {
            return new VsTestCase(dotNetTest.FullyQualifiedName, VsTestExecutor.Uri, assemblyPath)
            {
                CodeFilePath = dotNetTest.CodeFilePath,
                LineNumber = dotNetTest.LineNumber ?? -1
            };
        }

        public static VsTestResult ToVisualStudioType(this DotNetTestResult dotNetTestResult, string assemblyPath)
        {
            var testCase = dotNetTestResult.Test.ToVisualStudioType(assemblyPath);

            var testResult = new VsTestResult(testCase)
            {
                DisplayName = dotNetTestResult.DisplayName,
                Outcome = (VsTestOutcome)Enum.Parse(typeof(VsTestOutcome), dotNetTestResult.Outcome.ToString()),
                Duration = dotNetTestResult.Duration,
                ComputerName = Environment.MachineName,
                ErrorMessage = dotNetTestResult.ErrorMessage,
                ErrorStackTrace = dotNetTestResult.ErrorStackTrace
            };

            foreach (var message in dotNetTestResult.Messages)
                testResult.Messages.Add(new VsTestResultMessage(VsTestResultMessage.StandardOutCategory, message));

            return testResult;
        }
    }
}