namespace Fixie.Tests
{
    using System;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Assertions;
    using Fixie.Internal;

    public class TestClassConstructionTests
    {
        static string? FailingMember;
        static int? FailingMemberOccurrence;

        public TestClassConstructionTests()
        {
            FailingMember = null;
            FailingMemberOccurrence = null;
        }

        static void FailDuring(string failingMemberName, int? occurrence = null)
        {
            FailingMember = failingMemberName;
            FailingMemberOccurrence = occurrence;
        }

        static Output Run<TSampleTestClass, TExecution>() where TExecution : Execution, new()
            => Run<TExecution>(typeof(TSampleTestClass));

        static Output Run<TExecution>(Type testClass) where TExecution : Execution, new()
        {
            using var console = new RedirectedConsole();

            var results = Utility.Run<TExecution>(testClass);

            return new Output(console.Lines().ToArray(), results.ToArray());
        }

        class Output
        {
            readonly string[] lifecycle;
            readonly string[] results;

            public Output(string[] lifecycle, string[] results)
            {
                this.lifecycle = lifecycle;
                this.results = results;
            }

            public void ShouldHaveLifecycle(params string[] expected)
            {
                lifecycle.ShouldBe(expected);
            }

            public void ShouldHaveResults(params string[] expected)
            {
                var namespaceQualifiedExpectation = expected.Select(x => "Fixie.Tests.TestClassConstructionTests+" + x).ToArray();

                results.ShouldBe(namespaceQualifiedExpectation);
            }
        }

        class SampleTestClass : IDisposable
        {
            bool disposed;

            public SampleTestClass()
            {
                WhereAmI();
            }

            public void Fail()
            {
                WhereAmI();
                throw new FailureException();
            }

            [Input(1)]
            [Input(2)]
            public void Pass(int i)
            {
                WhereAmI(i);
            }

            public void Skip()
            {
                WhereAmI();
                throw new ShouldBeUnreachableException();
            }

            public void Dispose()
            {
                if (disposed)
                    throw new ShouldBeUnreachableException();
                disposed = true;

                WhereAmI();
            }
        }

        class AllSkippedTestClass : IDisposable
        {
            bool disposed;

            public AllSkippedTestClass()
            {
                WhereAmI();
            }

            public void SkipA()
            {
                WhereAmI();
                throw new ShouldBeUnreachableException();
            }

            public void SkipB()
            {
                WhereAmI();
                throw new ShouldBeUnreachableException();
            }

            public void SkipC()
            {
                WhereAmI();
                throw new ShouldBeUnreachableException();
            }

            public void Dispose()
            {
                if (disposed)
                    throw new ShouldBeUnreachableException();
                disposed = true;

                WhereAmI();
            }
        }

        static class StaticTestClass
        {
            public static void Fail()
            {
                WhereAmI();
                throw new FailureException();
            }

            public static void Pass()
            {
                WhereAmI();
            }

            public static void Skip()
            {
                WhereAmI();
                throw new ShouldBeUnreachableException();
            }
        }

        static void WhereAmI(object parameter, [CallerMemberName] string member = default!)
        {
            System.Console.WriteLine($"{member}({parameter})");

            ProcessScriptedFailure(member);
        }

        static void WhereAmI([CallerMemberName] string member = default!)
        {
            System.Console.WriteLine(member);

            ProcessScriptedFailure(member);
        }

        static void ProcessScriptedFailure(string member)
        {
            if (FailingMember == member)
            {
                if (FailingMemberOccurrence == null)
                    throw new FailureException(member);

                if (FailingMemberOccurrence > 0)
                {
                    FailingMemberOccurrence--;

                    if (FailingMemberOccurrence == 0)
                        throw new FailureException(member);
                }
            }
        }

        static bool ShouldSkip(TestMethod test)
            => test.Method.Name.Contains("Skip");

        class CreateInstancePerCase : Execution
        {
            public void Execute(TestClass testClass)
            {
                foreach (var test in testClass.Tests)
                    if (!ShouldSkip(test))
                        test.RunCases(Utility.UsingInputAttributes, @case => CaseInspection());
            }
        }

        class CreateInstancePerClass : Execution
        {
            public void Execute(TestClass testClass)
            {
                var type = testClass.Type;
                var instance = type.IsStatic() ? null : Activator.CreateInstance(type);

                foreach (var test in testClass.Tests)
                    if (!ShouldSkip(test))
                        test.RunCases(Utility.UsingInputAttributes, instance, @case => CaseInspection());

                instance.Dispose();
            }
        }

        static void CaseInspection() => WhereAmI();

        public void ShouldConstructPerCaseByDefault()
        {
            //NOTE: With no input parameter or skip behaviors,
            //      all test methods are attempted once and with zero
            //      parameters, so Skip() is reached and Pass(int)
            //      is attempted once but never reached.

            var output = Run<SampleTestClass, DefaultExecution>();

            output.ShouldHaveResults(
                "SampleTestClass.Fail failed: 'Fail' failed!",
                "SampleTestClass.Pass failed: Parameter count mismatch.",
                "SampleTestClass.Skip failed: 'Skip' reached a line of code thought to be unreachable.");

            output.ShouldHaveLifecycle(
                ".ctor", "Fail", "Dispose",
                ".ctor", "Dispose",
                ".ctor", "Skip", "Dispose");
        }

        public void ShouldAllowConstructingPerCase()
        {
            var output = Run<SampleTestClass, CreateInstancePerCase>();

            output.ShouldHaveResults(
                "SampleTestClass.Fail failed: 'Fail' failed!",
                "SampleTestClass.Pass(1) passed",
                "SampleTestClass.Pass(2) passed",
                "SampleTestClass.Skip skipped");

            output.ShouldHaveLifecycle(
                ".ctor", "Fail", "CaseInspection", "Dispose",
                ".ctor", "Pass(1)", "CaseInspection", "Dispose",
                ".ctor", "Pass(2)", "CaseInspection", "Dispose");
        }

        public void ShouldFailCaseInAbsenseOfPrimaryCaseResultAndProceedWithCaseInspectionWhenConstructingPerCaseAndConstructorThrows()
        {
            FailDuring(".ctor");
            
            var output = Run<SampleTestClass, CreateInstancePerCase>();

            output.ShouldHaveResults(
                "SampleTestClass.Fail failed: '.ctor' failed!",
                "SampleTestClass.Pass(1) failed: '.ctor' failed!",
                "SampleTestClass.Pass(2) failed: '.ctor' failed!",
                "SampleTestClass.Skip skipped");

            output.ShouldHaveLifecycle(
                ".ctor", "CaseInspection",
                ".ctor", "CaseInspection",
                ".ctor", "CaseInspection");
        }

        public void ShouldFailCaseWithoutHidingPrimaryCaseResultAndProceedWithCaseInspectionWhenConstructingPerCaseAndDisposeThrows()
        {
            FailDuring("Dispose");

            var output = Run<SampleTestClass, CreateInstancePerCase>();

            output.ShouldHaveResults(
                "SampleTestClass.Fail failed: 'Fail' failed!",
                "SampleTestClass.Fail failed: 'Dispose' failed!",
                "SampleTestClass.Pass(1) passed",
                "SampleTestClass.Pass(1) failed: 'Dispose' failed!",
                "SampleTestClass.Pass(2) passed",
                "SampleTestClass.Pass(2) failed: 'Dispose' failed!",
                "SampleTestClass.Skip skipped");

            output.ShouldHaveLifecycle(
                ".ctor", "Fail", "CaseInspection", "Dispose",
                ".ctor", "Pass(1)", "CaseInspection", "Dispose",
                ".ctor", "Pass(2)", "CaseInspection", "Dispose");
        }

        public void ShouldAllowConstructingPerClass()
        {
            var output = Run<SampleTestClass, CreateInstancePerClass>();

            output.ShouldHaveResults(
                "SampleTestClass.Fail failed: 'Fail' failed!",
                "SampleTestClass.Pass(1) passed",
                "SampleTestClass.Pass(2) passed",
                "SampleTestClass.Skip skipped");

            output.ShouldHaveLifecycle(
                ".ctor",
                "Fail", "CaseInspection",
                "Pass(1)", "CaseInspection",
                "Pass(2)", "CaseInspection",
                "Dispose");
        }

        public void ShouldFailAllTestsWithoutHidingPrimarySkipResultsWhenConstructingPerClassAndConstructorThrows()
        {
            FailDuring(".ctor");

            var output = Run<SampleTestClass, CreateInstancePerClass>();

            output.ShouldHaveResults(
                "SampleTestClass.Fail skipped",
                "SampleTestClass.Fail failed: Exception has been thrown by the target of an invocation.\r\n------- Inner Exception: Fixie.Tests.FailureException -------\r\n'.ctor' failed!",
                "SampleTestClass.Pass skipped",
                "SampleTestClass.Pass failed: Exception has been thrown by the target of an invocation.\r\n------- Inner Exception: Fixie.Tests.FailureException -------\r\n'.ctor' failed!",
                "SampleTestClass.Skip skipped",
                "SampleTestClass.Skip failed: Exception has been thrown by the target of an invocation.\r\n------- Inner Exception: Fixie.Tests.FailureException -------\r\n'.ctor' failed!"
            );

            output.ShouldHaveLifecycle(".ctor");
        }

        public void ShouldFailAllTestsWithoutHidingPrimaryCaseResultsWhenConstructingPerClassAndDisposeThrows()
        {
            FailDuring("Dispose");

            var output = Run<SampleTestClass, CreateInstancePerClass>();

            output.ShouldHaveResults(
                "SampleTestClass.Fail failed: 'Fail' failed!",
                "SampleTestClass.Pass(1) passed",
                "SampleTestClass.Pass(2) passed",
                "SampleTestClass.Fail failed: 'Dispose' failed!",
                "SampleTestClass.Pass failed: 'Dispose' failed!",
                "SampleTestClass.Skip skipped",
                "SampleTestClass.Skip failed: 'Dispose' failed!");

            output.ShouldHaveLifecycle(
                ".ctor",
                "Fail", "CaseInspection",
                "Pass(1)", "CaseInspection",
                "Pass(2)", "CaseInspection",
                "Dispose");
        }

        public void ShouldBypassConstructionWhenConstructingPerCaseAndAllCasesAreSkipped()
        {
            var output = Run<AllSkippedTestClass, CreateInstancePerCase>();

            output.ShouldHaveResults(
                "AllSkippedTestClass.SkipA skipped",
                "AllSkippedTestClass.SkipB skipped",
                "AllSkippedTestClass.SkipC skipped");

            output.ShouldHaveLifecycle();
        }

        public void ShouldNotBypassConstructionWhenConstructingPerClassAndAllCasesAreSkipped()
        {
            var output = Run<AllSkippedTestClass, CreateInstancePerClass>();

            output.ShouldHaveResults(
                "AllSkippedTestClass.SkipA skipped",
                "AllSkippedTestClass.SkipB skipped",
                "AllSkippedTestClass.SkipC skipped");

            output.ShouldHaveLifecycle(".ctor", "Dispose");
        }

        public void ShouldBypassConstructionAttemptsWhenTestMethodsAreStatic()
        {
            var output = Run<DefaultExecution>(typeof(StaticTestClass));

            output.ShouldHaveResults(
                "StaticTestClass.Fail failed: 'Fail' failed!",
                "StaticTestClass.Pass passed",
                "StaticTestClass.Skip failed: 'Skip' reached a line of code thought to be unreachable."
            );

            output.ShouldHaveLifecycle("Fail", "Pass", "Skip");


            output = Run<CreateInstancePerCase>(typeof(StaticTestClass));

            output.ShouldHaveResults(
                "StaticTestClass.Fail failed: 'Fail' failed!",
                "StaticTestClass.Pass passed",
                "StaticTestClass.Skip skipped"
            );

            output.ShouldHaveLifecycle(
                "Fail", "CaseInspection",
                "Pass", "CaseInspection");


            output = Run<CreateInstancePerClass>(typeof(StaticTestClass));

            output.ShouldHaveResults(
                "StaticTestClass.Fail failed: 'Fail' failed!",
                "StaticTestClass.Pass passed",
                "StaticTestClass.Skip skipped"
            );

            output.ShouldHaveLifecycle(
                "Fail", "CaseInspection",
                "Pass", "CaseInspection");
        }
    }
}