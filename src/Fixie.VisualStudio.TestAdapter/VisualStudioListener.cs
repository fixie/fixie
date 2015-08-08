using System;
using Fixie.Execution;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

namespace Fixie.VisualStudio.TestAdapter
{
    public class VisualStudioListener : LongLivedMarshalByRefObject, Listener
    {
        //The Visual Studio test runner has poor support for parameterized test methods,
        //when the arguments are not known ahead of time. It assumes that the TestCase
        //FullyQualifyName strings will pefectly match between the discovery phase and
        //the execution phase. Otherwise, you get a glitchy experience as Visual Studio
        //tries and fails to match up actual execution results with the list of tests
        //found at discovery time.

        //The best thing that can be done for parameterized tests, then, is to include
        //the "Method Group" (full name of the class/method without parameter
        //information) as the TestCase's FullyQualifiedName while providing the full Case
        //Name including parameter information as the TestResult's DisplayName. This
        //combination allows the Visual Studio test runner to display each individual
        //test case's success or failure, grouping parameterized cases under the method
        //name, while avoiding glitches for dynamically-generated test case parameters.

        readonly ITestExecutionRecorder log;
        readonly string assemblyPath;

        public VisualStudioListener(ITestExecutionRecorder log, string assemblyPath)
        {
            this.log = log;
            this.assemblyPath = assemblyPath;
        }

        public void AssemblyStarted(AssemblyInfo assembly) { }

        public void CaseSkipped(SkipResult result)
        {
            log.RecordResult(new TestResult(TestCase(result.MethodGroup))
            {
                DisplayName = result.Name,
                Outcome = Map(CaseStatus.Skipped),
                ComputerName = Environment.MachineName
            });
        }

        public void CasePassed(PassResult result)
        {
            var testResult = new TestResult(TestCase(result.MethodGroup))
            {
                DisplayName = result.Name,
                Outcome = Map(CaseStatus.Passed),
                Duration = result.Duration,
                ComputerName = Environment.MachineName
            };

            AttachCapturedConsoleOutput(result.Output, testResult);

            log.RecordResult(testResult);
        }

        public void CaseFailed(FailResult result)
        {
            var testResult = new TestResult(TestCase(result.MethodGroup))
            {
                DisplayName = result.Name,
                Outcome = Map(CaseStatus.Failed),
                Duration = result.Duration,
                ComputerName = Environment.MachineName,
                ErrorMessage = result.Exceptions.PrimaryException.DisplayName,
                ErrorStackTrace = result.Exceptions.CompoundStackTrace
            };

            AttachCapturedConsoleOutput(result.Output, testResult);

            log.RecordResult(testResult);
        }

        public void AssemblyCompleted(AssemblyInfo assembly, AssemblyResult result)
        {
            log.Info(result.Summary);
        }

        TestCase TestCase(MethodGroup methodGroup)
        {
            return new TestCase(methodGroup.FullName, VsTestExecutor.Uri, assemblyPath);
        }

        static TestOutcome Map(CaseStatus caseStatus)
        {
            switch (caseStatus)
            {
                case CaseStatus.Passed:
                    return TestOutcome.Passed;
                case CaseStatus.Failed:
                    return TestOutcome.Failed;
                case CaseStatus.Skipped:
                    return TestOutcome.Skipped;
                default:
                    return TestOutcome.None;
            }
        }

        static void AttachCapturedConsoleOutput(string output, TestResult testResult)
        {
            if (!String.IsNullOrEmpty(output))
                testResult.Messages.Add(new TestResultMessage(TestResultMessage.StandardOutCategory, output));
        }
    }
}