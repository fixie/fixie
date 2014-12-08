using System;
using System.Text;
using Fixie.Execution;
using Fixie.Results;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

namespace Fixie.VisualStudio.TestAdapter
{
    public class VisualStudioListener : MarshalByRefObject, Listener
    {
        readonly ITestExecutionRecorder log;
        readonly string source;

        public VisualStudioListener(ITestExecutionRecorder log, string source)
        {
            this.log = log;
            this.source = source;
        }

        public void AssemblyStarted(AssemblyInfo assembly) { }

        public void CaseSkipped(SkipResult result)
        {
            var testCase = new TestCase(TestMethodName(result.Name), Executor.Uri, source);
            log.RecordResult(new TestResult(testCase)
            {
                DisplayName = result.Name,
                Outcome = Map(CaseStatus.Passed),
                ComputerName = Environment.MachineName
            });
        }

        public void CasePassed(PassResult result)
        {
            var testCase = new TestCase(TestMethodName(result.Name), Executor.Uri, source);
            log.RecordResult(new TestResult(testCase)
            {
                DisplayName = result.Name,
                Outcome = Map(CaseStatus.Passed),
                Duration = result.Duration,
                ComputerName = Environment.MachineName
            });
        }

        public void CaseFailed(FailResult result)
        {
            var testCase = new TestCase(TestMethodName(result.Name), Executor.Uri, source);
            log.RecordResult(new TestResult(testCase)
            {
                DisplayName = result.Name,
                Outcome = Map(CaseStatus.Failed),
                Duration = result.Duration,
                ComputerName = Environment.MachineName,
                ErrorMessage = result.Exceptions.PrimaryException.DisplayName,
                ErrorStackTrace = result.Exceptions.CompoundStackTrace
            });
        }

        public void AssemblyCompleted(AssemblyInfo assembly, AssemblyResult result)
        {
            var assemblyName = typeof(Listener).Assembly.GetName();
            var name = assemblyName.Name;
            var version = assemblyName.Version;

            var line = new StringBuilder();

            line.AppendFormat("{0} passed", result.Passed);
            line.AppendFormat(", {0} failed", result.Failed);

            if (result.Skipped > 0)
                line.AppendFormat(", {0} skipped", result.Skipped);

            line.AppendFormat(", took {0:N2} seconds", result.Duration.TotalSeconds);

            line.AppendFormat(" ({0} {1}).", name, version);
            log.Info(line.ToString());
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

        static string TestMethodName(string caseName)
        {
            //The Visual Studio test runner has poor support for parameterized test methods,
            //when the arguments are not known ahead of time. It assumes that the TestCase
            //FullyQualifyName strings will pefectly match between the discovery phase and
            //the execution phase. The best thing that can be done for parameterized tests
            //is to include the full name of the class/method (without parameter information)
            //as the TestCase's FullyQualifiedName while providing the full Case name
            //(including parameter information) as the TestResult's DisplayName.  This
            //combination allows the Visual Studio test runner to display each individual
            //test case's success or failure, grouped under the method name.

            if (caseName.Contains("("))
                return caseName.Substring(0, caseName.IndexOf("("));

            return caseName;
        }
    }
}