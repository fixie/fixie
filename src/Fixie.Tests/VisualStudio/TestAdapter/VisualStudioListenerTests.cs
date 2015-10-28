using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using Fixie.Internal;
using Fixie.VisualStudio.TestAdapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using Should;

namespace Fixie.Tests.VisualStudio.TestAdapter
{
    public class VisualStudioListenerTests
    {
        public void ShouldReportResultsToExecutionRecorder()
        {
            const string assemblyPath = "assembly.path.dll";
            var recorder = new StubExecutionRecorder();

            using (var console = new RedirectedConsole())
            using (var executionSink = new ExecutionSink(recorder, assemblyPath))
            {
                var listener = new VisualStudioListener(executionSink);
                var convention = SelfTestConvention.Build();
                convention.CaseExecution.Skip(x => x.Method.Has<SkipAttribute>(), x => x.Method.GetCustomAttribute<SkipAttribute>().Reason);
                convention.Parameters.Add<InputAttributeParameterSource>();

                typeof(PassFailTestClass).Run(listener, convention);

                var testClass = typeof(PassFailTestClass).FullName;

                var summaryMessage = recorder.Messages.Single();
                summaryMessage.Level.ShouldEqual(TestMessageLevel.Informational);
                CleanBrittleValues(summaryMessage.Message).ShouldEqual("1 passed, 1 failed, 2 skipped, took 1.23 seconds (Fixie 1.2.3.4).");

                console.Lines()
                    .ShouldEqual(
                        "Console.Out: Fail",
                        "Console.Error: Fail",
                        "Console.Out: Pass",
                        "Console.Error: Pass");

                var results = recorder.TestResults;
                results.Count.ShouldEqual(4);

                foreach (var result in results)
                {
                    result.Traits.ShouldBeEmpty();
                    result.Attachments.ShouldBeEmpty();
                    result.ComputerName.ShouldEqual(Environment.MachineName);
                    result.TestCase.Traits.ShouldBeEmpty();
                    result.TestCase.LocalExtensionData.ShouldBeNull();
                    result.TestCase.Source.ShouldEqual("assembly.path.dll");

                    //Source locations are a discovery-time concern.
                    result.TestCase.CodeFilePath.ShouldBeNull();
                    result.TestCase.LineNumber.ShouldEqual(-1);
                }
                
                results[0].TestCase.FullyQualifiedName.ShouldEqual(testClass +".SkipWithReason");
                results[0].TestCase.DisplayName.ShouldEqual(testClass +".SkipWithReason");
                results[0].TestCase.ExecutorUri.ToString().ShouldEqual("executor://fixie.visualstudio/");
                results[0].Outcome.ShouldEqual(TestOutcome.Skipped);
                results[0].ErrorMessage.ShouldEqual("Skipped with reason.");
                results[0].ErrorStackTrace.ShouldBeNull();
                results[0].DisplayName.ShouldEqual(testClass + ".SkipWithReason");
                results[0].Messages.ShouldBeEmpty();
                results[0].Duration.ShouldEqual(TimeSpan.Zero);

                results[1].TestCase.FullyQualifiedName.ShouldEqual(testClass +".SkipWithoutReason");
                results[1].TestCase.DisplayName.ShouldEqual(testClass +".SkipWithoutReason");
                results[1].TestCase.ExecutorUri.ToString().ShouldEqual("executor://fixie.visualstudio/");
                results[1].Outcome.ShouldEqual(TestOutcome.Skipped);
                results[1].ErrorMessage.ShouldBeNull();
                results[1].ErrorStackTrace.ShouldBeNull();
                results[1].DisplayName.ShouldEqual(testClass +".SkipWithoutReason");
                results[1].Messages.ShouldBeEmpty();
                results[1].Duration.ShouldEqual(TimeSpan.Zero);

                results[2].TestCase.FullyQualifiedName.ShouldEqual(testClass +".Fail");
                results[2].TestCase.DisplayName.ShouldEqual(testClass +".Fail");
                results[2].TestCase.ExecutorUri.ToString().ShouldEqual("executor://fixie.visualstudio/");
                results[2].Outcome.ShouldEqual(TestOutcome.Failed);
                results[2].ErrorMessage.ShouldEqual("Fixie.Tests.FailureException");
                results[2].ErrorStackTrace.Lines().Select(CleanBrittleValues)
                    .ShouldEqual("'Fail' failed!",
                         "   at Fixie.Tests.VisualStudio.TestAdapter.VisualStudioListenerTests.PassFailTestClass.Fail() in " + PathToThisFile() + ":line #");
                results[2].DisplayName.ShouldEqual(testClass +".Fail");
                results[2].Messages.Count.ShouldEqual(1);
                results[2].Messages[0].Category.ShouldEqual(TestResultMessage.StandardOutCategory);
                results[2].Messages[0].Text.Lines().ShouldEqual("Console.Out: Fail", "Console.Error: Fail");
                results[2].Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);

                results[3].TestCase.FullyQualifiedName.ShouldEqual(testClass +".Pass");
                results[3].TestCase.DisplayName.ShouldEqual(testClass +".Pass");
                results[3].TestCase.ExecutorUri.ToString().ShouldEqual("executor://fixie.visualstudio/");
                results[3].Outcome.ShouldEqual(TestOutcome.Passed);
                results[3].ErrorMessage.ShouldBeNull();
                results[3].ErrorStackTrace.ShouldBeNull();
                results[3].DisplayName.ShouldEqual(testClass +".Pass(123)");
                results[3].Messages.Count.ShouldEqual(1);
                results[3].Messages[0].Category.ShouldEqual(TestResultMessage.StandardOutCategory);
                results[3].Messages[0].Text.Lines().ShouldEqual("Console.Out: Pass", "Console.Error: Pass");
                results[3].Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
            }
        }

        class InputAttributeParameterSource : ParameterSource
        {
            public IEnumerable<object[]> GetParameters(MethodInfo method)
            {
                return method.GetCustomAttributes<InputAttribute>().Select(x => x.Parameters);
            }
        }

        static string CleanBrittleValues(string actualRawContent)
        {
            //Avoid brittle assertion introduced by fixie version.
            var cleaned = Regex.Replace(actualRawContent, @"\(Fixie \d+\.\d+\.\d+\.\d+\)", @"(Fixie 1.2.3.4)");

            //Avoid brittle assertion introduced by test duration.
            var decimalSeparator = Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            cleaned = Regex.Replace(cleaned, @"took [\d" + Regex.Escape(decimalSeparator) + @"]+ seconds", @"took 1.23 seconds");

            //Avoid brittle assertion introduced by stack trace line numbers.
            cleaned = Regex.Replace(cleaned, @":line \d+", ":line #");

            return cleaned;
        }

        class StubExecutionRecorder : ITestExecutionRecorder
        {
            public class SentMessage
            {
                public TestMessageLevel Level { get; set; }
                public string Message { get; set; }
            }

            public List<SentMessage> Messages { get; private set; } 

            public List<TestResult> TestResults { get; private set; }

            public StubExecutionRecorder()
            {
                Messages = new List<SentMessage>();
                TestResults = new List<TestResult>();
            }

            public void SendMessage(TestMessageLevel testMessageLevel, string message)
            {
                Messages.Add(new SentMessage { Level = testMessageLevel, Message = message });
            }

            public void RecordResult(TestResult testResult)
            {
                TestResults.Add(testResult);
            }

            public void RecordStart(TestCase testCase)
            {
                throw new NotImplementedException();
            }

            public void RecordEnd(TestCase testCase, TestOutcome outcome)
            {
                throw new NotImplementedException();
            }

            public void RecordAttachments(IList<AttachmentSet> attachmentSets)
            {
                throw new NotImplementedException();
            }
        }

        static string PathToThisFile([CallerFilePath] string path = null)
        {
            return path;
        }

        class PassFailTestClass
        {
            [Input(123)]
            public void Pass(int x)
            {
                WhereAmI();
            }

            public void Fail()
            {
                WhereAmI();
                throw new FailureException();
            }

            [Skip]
            public void SkipWithoutReason() { throw new ShouldBeUnreachableException(); }

            [Skip("Skipped with reason.")]
            public void SkipWithReason() { throw new ShouldBeUnreachableException(); }

            static void WhereAmI([CallerMemberName] string member = null)
            {
                Console.Out.WriteLine("Console.Out: " + member);
                Console.Error.WriteLine("Console.Error: " + member);
            }
        }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
        class InputAttribute : Attribute
        {
            public InputAttribute(params object[] parameters)
            {
                Parameters = parameters;
            }

            public object[] Parameters { get; private set; }
        }

        [AttributeUsage(AttributeTargets.Method)]
        class SkipAttribute : Attribute
        {
            public SkipAttribute()
            {
            }

            public SkipAttribute(string reason)
            {
                Reason = reason;
            }

            public string Reason { get; private set; }
        }
    }
}