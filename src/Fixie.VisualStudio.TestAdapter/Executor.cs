using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

namespace Fixie.VisualStudio.TestAdapter
{
    [ExtensionUri(Id)]
    public class Executor : ITestExecutor
    {
        public const string Id = "executor://Fixie.VisualStudio";
        public static Uri Uri = new Uri(Id);

        public void RunTests(IEnumerable<TestCase> tests, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            foreach (var test in tests)
            {
                frameworkHandle.RecordResult(ToVsTestResult(test));
            }
        }

        public void RunTests(IEnumerable<string> sources, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            foreach (var assemblyPath in sources)
            {
                var test = new TestCase("From source " + assemblyPath, Uri, assemblyPath);
                frameworkHandle.RecordResult(new TestResult(test)
                {
                    Duration = TimeSpan.FromSeconds(2),
                    DisplayName = test.DisplayName,
                    Outcome = TestOutcome.Passed
                });
                frameworkHandle.RecordResult(new TestResult(test)
                {
                    Duration = TimeSpan.FromSeconds(2),
                    DisplayName = test.DisplayName,
                    Outcome = TestOutcome.Failed
                });
            }
        }

        public void Cancel()
        {
        }

        static TestResult ToVsTestResult(TestCase test)
        {
            return new TestResult(test)
            {
                Duration = TimeSpan.FromSeconds(2),
                DisplayName = test.DisplayName,
                Outcome = (TestOutcome)Enum.Parse(typeof(TestOutcome), test.DisplayName)
            };
        }
    }
}