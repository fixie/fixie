namespace Fixie.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using Assertions;
    using Fixie.Internal;

    public class LifecycleTests
    {
        static string? FailingMember;
        static int? FailingMemberOccurrence;

        readonly Discovery discovery;

        public LifecycleTests()
        {
            FailingMember = null;
            FailingMemberOccurrence = null;

            discovery = new SelfTestDiscovery();
        }

        static void FailDuring(string failingMemberName, int? occurrence = null)
        {
            FailingMember = failingMemberName;
            FailingMemberOccurrence = occurrence;
        }

        Output Run<TSampleTestClass, TExecution>() where TExecution : Execution, new()
        {
            return Run<TExecution>(typeof(TSampleTestClass));
        }

        Output Run<TSampleTestClass>(Execution execution)
        {
            return Run(typeof(TSampleTestClass), execution);
        }

        Output Run<TExecution>(Type testClass) where TExecution : Execution, new()
        {
            return Run(testClass, new TExecution());
        }

        Output Run<TExecution>(Type testClass, TExecution execution) where TExecution : Execution
        {
            var listener = new StubListener();
            using var console = new RedirectedConsole();

            Utility.Run(listener, discovery, execution, testClass);

            return new Output(console.Lines().ToArray(), listener.Entries.ToArray());
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
                var namespaceQualifiedExpectation = expected.Select(x => "Fixie.Tests.LifecycleTests+" + x).ToArray();

                results.ShouldBe(namespaceQualifiedExpectation);
            }
        }

        class SampleTestClass
        {
            [Input(1)]
            [Input(2)]
            public void Pass(int i)
            {
                WhereAmI(i);
            }

            public void Fail()
            {
                WhereAmI();
                throw new FailureException();
            }

            public void Skip()
            {
                WhereAmI();
                throw new ShouldBeUnreachableException();
            }
        }

        class AllSkippedTestClass
        {
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
        }

        static class StaticTestClass
        {
            public static void Pass()
            {
                WhereAmI();
            }

            public static void Fail()
            {
                WhereAmI();
                throw new FailureException();
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

        class InstrumentedExecution : Execution
        {
            readonly ParameterSource parameterSource;

            public InstrumentedExecution()
                : this(UsingInputAttributes) { }

            public InstrumentedExecution(ParameterSource parameterSource)
                => this.parameterSource = parameterSource;

            public void Execute(TestClass testClass)
            {
                ClassSetUp();

                foreach (var test in testClass.Tests)
                    if (!test.Method.Name.Contains("Skip"))
                        TestLifecycle(test);

                ClassTearDown();
            }

            void TestLifecycle(TestMethod test)
            {
                try
                {
                    TestSetUp();

                    var cases = test.HasParameters
                        ? parameterSource(test.Method)
                        : InvokeOnceWithZeroParameters;

                    foreach (var parameters in cases)
                        CaseLifecycle(test, parameters);

                    TestTearDown();
                }
                catch (Exception exception)
                {
                    test.Fail(exception);
                }
            }

            static void CaseLifecycle(TestMethod test, object?[] parameters)
            {
                try
                {
                    CaseSetUp();
                    test.Run(parameters, @case => CaseInspection());
                    CaseTearDown();
                }
                catch (Exception exception)
                {
                    test.Fail(exception);
                }
            }

            static readonly object[] EmptyParameters = {};
            static readonly object[][] InvokeOnceWithZeroParameters = { EmptyParameters };
        }

        static void ClassSetUp() => WhereAmI();
        static void TestSetUp() => WhereAmI();
        static void CaseSetUp() => WhereAmI();
        static void CaseTearDown() => WhereAmI();
        static void CaseInspection() => WhereAmI();
        static void TestTearDown() => WhereAmI();
        static void ClassTearDown() => WhereAmI();

        static IEnumerable<object?[]> UsingInputAttributes(MethodInfo method)
            => method
                .GetCustomAttributes<InputAttribute>(true)
                .Select(input => input.Parameters);

        class ShortCircuitTestExecution : Execution
        {
            public void Execute(TestClass testClass)
            {
                //Class lifecycle chooses not to invoke test.Run(...).
                //Since the tests never run, they are all considered
                //'skipped'.
            }
        }

        class RetryExecution : Execution
        {
            const int MaxAttempts = 3;

            public void Execute(TestClass testClass)
            {
                foreach (var test in testClass.Tests)
                    if (!test.Method.Name.Contains("Skip"))
                        foreach (var parameters in Cases(test))
                            RunWithRetries(test, parameters);
            }

            static void RunWithRetries(TestMethod test, object?[] parameters)
            {
                var remainingAttempts = MaxAttempts;

                while (remainingAttempts > 0)
                {
                    test.Run(parameters, @case =>
                    {
                        remainingAttempts--;
                        
                        if (@case.State == CaseState.Failed && remainingAttempts > 0)
                            @case.Skip(@case.Exception?.Message + " Retrying...");
                        else
                            remainingAttempts = 0;
                    });
                }
            }

            static IEnumerable<object?[]> Cases(TestMethod test)
            {
                if (test.HasParameters)
                {
                    foreach (var parameters in UsingInputAttributes(test.Method))
                        yield return parameters;
                }
                else
                {
                    yield return EmptyParameters;
                }
            }

            static readonly object[] EmptyParameters = {};
        }

        public void ShouldRunAllTestsByDefault()
        {
            var output = Run<SampleTestClass, DefaultExecution>();

            //NOTE: With no input parameter or skip behaviors,
            //      all test methods are attempted and with zero
            //      parameters, so Skip() is reached and Pass(int)
            //      is attempted but never reached.

            output.ShouldHaveResults(
                "SampleTestClass.Fail failed: 'Fail' failed!",
                "SampleTestClass.Pass failed: Parameter count mismatch.",
                "SampleTestClass.Skip failed: 'Skip' reached a line of code thought to be unreachable.");

            output.ShouldHaveLifecycle("Fail", "Skip");
        }

        public void ShouldSupportExecutionHooksAtClassAndTestAndCaseLevels()
        {
            var output = Run<SampleTestClass, InstrumentedExecution>();

            output.ShouldHaveResults(
                "SampleTestClass.Fail failed: 'Fail' failed!",
                "SampleTestClass.Pass(1) passed",
                "SampleTestClass.Pass(2) passed",
                "SampleTestClass.Skip skipped");

            output.ShouldHaveLifecycle(
                "ClassSetUp",
                "TestSetUp",
                "CaseSetUp", "Fail", "CaseInspection", "CaseTearDown",
                "TestTearDown",
                "TestSetUp",
                "CaseSetUp", "Pass(1)", "CaseInspection", "CaseTearDown",
                "CaseSetUp", "Pass(2)", "CaseInspection", "CaseTearDown",
                "TestTearDown",
                "ClassTearDown");
        }

        public void ShouldSupportStaticTestClassesAndMethods()
        {
            var output = Run<InstrumentedExecution>(typeof(StaticTestClass));

            output.ShouldHaveResults(
                "StaticTestClass.Fail failed: 'Fail' failed!",
                "StaticTestClass.Pass passed",
                "StaticTestClass.Skip skipped");

            output.ShouldHaveLifecycle(
                "ClassSetUp",
                "TestSetUp", "CaseSetUp", "Fail", "CaseInspection", "CaseTearDown", "TestTearDown",
                "TestSetUp", "CaseSetUp", "Pass", "CaseInspection", "CaseTearDown", "TestTearDown",
                "ClassTearDown");
        }

        public void ShouldFailAllTestsWithoutHidingPrimarySkipResultsWhenClassSetUpThrows()
        {
            FailDuring("ClassSetUp");
        
            var output = Run<SampleTestClass, InstrumentedExecution>();
        
            output.ShouldHaveResults(
                "SampleTestClass.Fail skipped",
                "SampleTestClass.Fail failed: 'ClassSetUp' failed!",
                
                "SampleTestClass.Pass skipped",
                "SampleTestClass.Pass failed: 'ClassSetUp' failed!",
                
                "SampleTestClass.Skip skipped",
                "SampleTestClass.Skip failed: 'ClassSetUp' failed!");
        
            output.ShouldHaveLifecycle("ClassSetUp");
        }

        public void ShouldFailTestWhenTestSetUpThrows()
        {
            FailDuring("TestSetUp", occurrence: 2);

            var output = Run<SampleTestClass, InstrumentedExecution>();

            output.ShouldHaveResults(
                "SampleTestClass.Fail failed: 'Fail' failed!",
                "SampleTestClass.Pass failed: 'TestSetUp' failed!",
                "SampleTestClass.Skip skipped");

            output.ShouldHaveLifecycle(
                "ClassSetUp",
                "TestSetUp",
                "CaseSetUp", "Fail", "CaseInspection", "CaseTearDown",
                "TestTearDown",
                "TestSetUp",
                "ClassTearDown");
        }

        public void ShouldFailTestWhenCustomParameterGenerationThrows()
        {
            var execution = new InstrumentedExecution(method =>
                throw new Exception("Failed to yield input parameters."));
            var output = Run<SampleTestClass>(execution);

            output.ShouldHaveResults(
                "SampleTestClass.Fail failed: 'Fail' failed!",
                "SampleTestClass.Pass failed: Failed to yield input parameters.",
                "SampleTestClass.Skip skipped");

            output.ShouldHaveLifecycle(
                "ClassSetUp",
                "TestSetUp",
                "CaseSetUp", "Fail", "CaseInspection", "CaseTearDown",
                "TestTearDown",
                "TestSetUp",
                "ClassTearDown");
        }

        public void ShouldFailTestWhenCaseSetUpThrows()
        {
            FailDuring("CaseSetUp", occurrence: 2);

            var output = Run<SampleTestClass, InstrumentedExecution>();

            output.ShouldHaveResults(
                "SampleTestClass.Fail failed: 'Fail' failed!",
                "SampleTestClass.Pass failed: 'CaseSetUp' failed!",
                "SampleTestClass.Pass(2) passed",
                "SampleTestClass.Skip skipped");

            output.ShouldHaveLifecycle(
                "ClassSetUp",
                "TestSetUp",
                "CaseSetUp", "Fail", "CaseInspection", "CaseTearDown",
                "TestTearDown",
                "TestSetUp",
                "CaseSetUp",
                "CaseSetUp", "Pass(2)", "CaseInspection", "CaseTearDown",
                "TestTearDown",
                "ClassTearDown");
        }

        public void ShouldFailCaseWithoutHidingPrimaryCaseResultsWhenCaseInspectionThrows()
        {
            FailDuring("CaseInspection");

            var output = Run<SampleTestClass, InstrumentedExecution>();

            output.ShouldHaveResults(
                "SampleTestClass.Fail failed: 'Fail' failed!",
                "SampleTestClass.Fail failed: 'CaseInspection' failed!",
                "SampleTestClass.Pass(1) passed",
                "SampleTestClass.Pass(1) failed: 'CaseInspection' failed!",
                "SampleTestClass.Pass(2) passed",
                "SampleTestClass.Pass(2) failed: 'CaseInspection' failed!",
                "SampleTestClass.Skip skipped");

            output.ShouldHaveLifecycle(
                "ClassSetUp",
                "TestSetUp",
                "CaseSetUp", "Fail", "CaseInspection", "CaseTearDown",
                "TestTearDown",
                "TestSetUp",
                "CaseSetUp", "Pass(1)", "CaseInspection", "CaseTearDown",
                "CaseSetUp", "Pass(2)", "CaseInspection", "CaseTearDown",
                "TestTearDown",
                "ClassTearDown");
        }

        public void ShouldFailTestWithoutHidingPrimaryCaseResultsWhenCaseTearDownThrows()
        {
            FailDuring("CaseTearDown");

            var output = Run<SampleTestClass, InstrumentedExecution>();

            output.ShouldHaveResults(
                "SampleTestClass.Fail failed: 'Fail' failed!",
                "SampleTestClass.Fail failed: 'CaseTearDown' failed!",
                "SampleTestClass.Pass(1) passed",
                "SampleTestClass.Pass failed: 'CaseTearDown' failed!",
                "SampleTestClass.Pass(2) passed",
                "SampleTestClass.Pass failed: 'CaseTearDown' failed!",
                "SampleTestClass.Skip skipped");

            output.ShouldHaveLifecycle(
                "ClassSetUp",
                "TestSetUp",
                "CaseSetUp", "Fail", "CaseInspection", "CaseTearDown",
                "TestTearDown",
                "TestSetUp",
                "CaseSetUp", "Pass(1)", "CaseInspection", "CaseTearDown",
                "CaseSetUp", "Pass(2)", "CaseInspection", "CaseTearDown",
                "TestTearDown",
                "ClassTearDown");
        }

        public void ShouldFailTestWithoutHidingPrimaryCaseResultsWhenTestTearDownThrows()
        {
            FailDuring("TestTearDown");

            var output = Run<SampleTestClass, InstrumentedExecution>();

            output.ShouldHaveResults(
                "SampleTestClass.Fail failed: 'Fail' failed!",
                "SampleTestClass.Fail failed: 'TestTearDown' failed!",

                "SampleTestClass.Pass(1) passed",
                "SampleTestClass.Pass(2) passed",
                "SampleTestClass.Pass failed: 'TestTearDown' failed!",
                
                "SampleTestClass.Skip skipped");

            output.ShouldHaveLifecycle(
                "ClassSetUp",
                "TestSetUp",
                "CaseSetUp", "Fail", "CaseInspection", "CaseTearDown",
                "TestTearDown",
                "TestSetUp",
                "CaseSetUp", "Pass(1)", "CaseInspection", "CaseTearDown",
                "CaseSetUp", "Pass(2)", "CaseInspection", "CaseTearDown",
                "TestTearDown",
                "ClassTearDown");
        }

        public void ShouldFailAllTestsWithoutHidingPrimaryCaseResultsWhenClassTearDownThrows()
        {
            FailDuring("ClassTearDown");

            var output = Run<SampleTestClass, InstrumentedExecution>();

            output.ShouldHaveResults(
                "SampleTestClass.Fail failed: 'Fail' failed!",
                "SampleTestClass.Pass(1) passed",
                "SampleTestClass.Pass(2) passed",

                "SampleTestClass.Fail failed: 'ClassTearDown' failed!",
                "SampleTestClass.Pass failed: 'ClassTearDown' failed!",
                "SampleTestClass.Skip skipped",
                "SampleTestClass.Skip failed: 'ClassTearDown' failed!");

            output.ShouldHaveLifecycle(
                "ClassSetUp",
                "TestSetUp",
                "CaseSetUp", "Fail", "CaseInspection", "CaseTearDown",
                "TestTearDown",
                "TestSetUp",
                "CaseSetUp", "Pass(1)", "CaseInspection", "CaseTearDown",
                "CaseSetUp", "Pass(2)", "CaseInspection", "CaseTearDown",
                "TestTearDown",
                "ClassTearDown");
        }

        public void ShouldSkipTestLifecyclesWhenAllTestsAreSkipped()
        {
            var output = Run<AllSkippedTestClass, InstrumentedExecution>();

            output.ShouldHaveResults(
                "AllSkippedTestClass.SkipA skipped",
                "AllSkippedTestClass.SkipB skipped",
                "AllSkippedTestClass.SkipC skipped");

            output.ShouldHaveLifecycle("ClassSetUp", "ClassTearDown");
        }

        public void ShouldAllowRunningTestsMultipleTimesWithDistinctResultPerInvocation()
        {
            FailDuring("Pass", occurrence: 1);

            var output = Run<SampleTestClass, RetryExecution>();

            output.ShouldHaveResults(
                "SampleTestClass.Fail skipped: 'Fail' failed! Retrying...",
                "SampleTestClass.Fail skipped: 'Fail' failed! Retrying...",
                "SampleTestClass.Fail failed: 'Fail' failed!",
                "SampleTestClass.Pass(1) skipped: 'Pass' failed! Retrying...",
                "SampleTestClass.Pass(1) passed",
                "SampleTestClass.Pass(2) passed",
                "SampleTestClass.Skip skipped");

            output.ShouldHaveLifecycle("Fail", "Fail", "Fail", "Pass(1)", "Pass(1)", "Pass(2)");
        }

        public void ShouldSkipAllTestsWhenShortCircuitingTestExecution()
        {
            var output = Run<SampleTestClass, ShortCircuitTestExecution>();

            output.ShouldHaveResults(
                "SampleTestClass.Fail skipped",
                "SampleTestClass.Pass skipped",
                "SampleTestClass.Skip skipped");

            output.ShouldHaveLifecycle();
        }
    }
}