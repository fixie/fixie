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

        readonly Discovery discovery;

        public LifecycleTests()
        {
            FailingMember = null;
            discovery = new SelfTestDiscovery();
        }

        static void FailDuring(string failingMemberName)
        {
            FailingMember = failingMemberName;
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
            public void Pass()
            {
                WhereAmI();
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

        class ParameterizedSampleTestClass
        {
            public void IntArg(int i)
            {
                WhereAmI();
                if (i != 0)
                    throw new Exception("Expected 0, but was " + i);
            }

            public void BoolArg(bool b)
            {
                WhereAmI();
                if (!b)
                    throw new Exception("Expected true, but was false");
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

        static void WhereAmI([CallerMemberName] string member = default!)
        {
            System.Console.WriteLine(member);

            if (FailingMember == member)
                throw new FailureException(member);
        }

        class InstrumentedExecution : Execution
        {
            readonly ParameterSource parameterSource;

            public InstrumentedExecution()
                : this(new ParameterGenerator()) { }

            public InstrumentedExecution(ParameterSource parameterSource)
                => this.parameterSource = parameterSource;

            public void Execute(TestClass testClass)
            {
                ClassSetUp();
                testClass.RunTests(test =>
                {
                    if (test.Method.Name.Contains("Skip"))
                        return;

                    TestSetUp();
                    test.RunCases(parameterSource, @case =>
                    {
                        CaseSetUp();
                        @case.Execute();
                        CaseTearDown();
                    });
                    TestTearDown();
                });
                ClassTearDown();
            }
        }

        static void ClassSetUp() => WhereAmI();
        static void TestSetUp() => WhereAmI();
        static void CaseSetUp() => WhereAmI();
        static void CaseTearDown() => WhereAmI();
        static void TestTearDown() => WhereAmI();
        static void ClassTearDown() => WhereAmI();

        class ShortCircuitClassExecution : Execution
        {
            public void Execute(TestClass testClass)
            {
                //Class lifecycle chooses not to invoke testClass.RunTests(...).
                //Since the tests never run, they are all considered
                //'skipped'.
            }
        }

        class ShortCircuitTestExection : Execution
        {
            public void Execute(TestClass testClass)
            {
                testClass.RunTests(test =>
                {
                    //Test lifecycle chooses not to invoke test.RunCases(...).
                    //Since the tests never run, they are all considered
                    //'skipped'.
                });
            }
        }

        class ShortCircuitCaseExection : Execution
        {
            public void Execute(TestClass testClass)
            {
                testClass.RunTests(test =>
                {
                    test.RunCases(@case =>
                    {
                        //Case lifecycle chooses not to invoke @case.Execute(instance).
                        //Since the test cases never run, they are all considered
                        //'skipped'.
                    });
                });
            }
        }

        class RunHooksTwice : Execution
        {
            public void Execute(TestClass testClass)
            {
                testClass.RunTests(test =>
                {
                    if (test.Method.Name.Contains("Skip")) return;
                    test.RunCases(@case => { @case.Execute(); @case.Execute(); });
                    test.RunCases(@case => { @case.Execute(); @case.Execute(); });
                });

                testClass.RunTests(test =>
                {
                    if (test.Method.Name.Contains("Skip")) return;
                    test.RunCases(@case => { @case.Execute(); @case.Execute(); });
                    test.RunCases(@case => { @case.Execute(); @case.Execute(); });
                });
            }
        }

        class BuggyParameterSource : ParameterSource
        {
            public IEnumerable<object[]> GetParameters(MethodInfo method)
            {
                throw new Exception("Exception thrown while attempting to yield input parameters for method: " + method.Name);
            }
        }

        public void ShouldRunAllTestsByDefault()
        {
            var output = Run<SampleTestClass, DefaultExecution>();

            output.ShouldHaveResults(
                "SampleTestClass.Fail failed: 'Fail' failed!",
                "SampleTestClass.Pass passed",
                "SampleTestClass.Skip failed: 'Skip' reached a line of code thought to be unreachable.");

            output.ShouldHaveLifecycle("Fail", "Pass", "Skip");
        }

        public void ShouldSupportExecutionHooksAtClassAndTestAndCaseLevels()
        {
            var output = Run<SampleTestClass, InstrumentedExecution>();

            output.ShouldHaveResults(
                "SampleTestClass.Fail failed: 'Fail' failed!",
                "SampleTestClass.Pass passed",
                "SampleTestClass.Skip skipped");

            output.ShouldHaveLifecycle(
                "ClassSetUp",
                "TestSetUp", "CaseSetUp", "Fail", "CaseTearDown", "TestTearDown",
                "TestSetUp", "CaseSetUp", "Pass", "CaseTearDown", "TestTearDown",
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
                "TestSetUp", "CaseSetUp", "Fail", "CaseTearDown", "TestTearDown",
                "TestSetUp", "CaseSetUp", "Pass", "CaseTearDown", "TestTearDown",
                "ClassTearDown");
        }

        public void ShouldFailAllTestsWhenClassSetUpThrows()
        {
            FailDuring("ClassSetUp");
        
            var output = Run<SampleTestClass, InstrumentedExecution>();
        
            output.ShouldHaveResults(
                "SampleTestClass.Fail failed: 'ClassSetUp' failed!",
                "SampleTestClass.Pass failed: 'ClassSetUp' failed!",
                "SampleTestClass.Skip failed: 'ClassSetUp' failed!");
        
            output.ShouldHaveLifecycle("ClassSetUp");
        }

        public void ShouldFailTestWhenTestSetUpThrows()
        {
            FailDuring("TestSetUp");

            var output = Run<SampleTestClass, InstrumentedExecution>();

            output.ShouldHaveResults(
                "SampleTestClass.Fail failed: 'TestSetUp' failed!",
                "SampleTestClass.Pass failed: 'TestSetUp' failed!",
                "SampleTestClass.Skip skipped");

            output.ShouldHaveLifecycle(
                "ClassSetUp",
                "TestSetUp",
                "TestSetUp",
                "ClassTearDown");
        }

        public void ShouldFailCaseWhenCaseSetUpThrows()
        {
            FailDuring("CaseSetUp");

            var output = Run<SampleTestClass, InstrumentedExecution>();

            output.ShouldHaveResults(
                "SampleTestClass.Fail failed: 'CaseSetUp' failed!",
                "SampleTestClass.Pass failed: 'CaseSetUp' failed!",
                "SampleTestClass.Skip skipped");

            output.ShouldHaveLifecycle(
                "ClassSetUp",
                "TestSetUp", "CaseSetUp", "TestTearDown",
                "TestSetUp", "CaseSetUp", "TestTearDown",
                "ClassTearDown");
        }

        public void ShouldFailCaseWithoutHidingPrimaryFailureWhenCaseTearDownThrows()
        {
            FailDuring("CaseTearDown");

            var output = Run<SampleTestClass, InstrumentedExecution>();

            output.ShouldHaveResults(
                "SampleTestClass.Fail failed: 'Fail' failed!",
                "SampleTestClass.Fail failed: 'CaseTearDown' failed!",
                "SampleTestClass.Pass failed: 'CaseTearDown' failed!",
                "SampleTestClass.Skip skipped");

            output.ShouldHaveLifecycle(
                "ClassSetUp",
                "TestSetUp", "CaseSetUp", "Fail", "CaseTearDown", "TestTearDown",
                "TestSetUp", "CaseSetUp", "Pass", "CaseTearDown", "TestTearDown",
                "ClassTearDown");
        }

        public void ShouldFailTestWithoutHidingPrimaryCaseResultsWhenTestTearDownThrows()
        {
            FailDuring("TestTearDown");

            var output = Run<SampleTestClass, InstrumentedExecution>();

            output.ShouldHaveResults(
                "SampleTestClass.Fail failed: 'Fail' failed!",
                "SampleTestClass.Fail failed: 'TestTearDown' failed!",

                "SampleTestClass.Pass passed",
                "SampleTestClass.Pass failed: 'TestTearDown' failed!",
                
                "SampleTestClass.Skip skipped");

            output.ShouldHaveLifecycle(
                "ClassSetUp",
                "TestSetUp", "CaseSetUp", "Fail", "CaseTearDown", "TestTearDown",
                "TestSetUp", "CaseSetUp", "Pass", "CaseTearDown", "TestTearDown",
                "ClassTearDown");
        }

        public void ShouldFailAllTestsWithoutHidingPrimaryCaseResultsWhenClassTearDownThrows()
        {
            FailDuring("ClassTearDown");

            var output = Run<SampleTestClass, InstrumentedExecution>();

            output.ShouldHaveResults(
                "SampleTestClass.Fail failed: 'Fail' failed!",
                "SampleTestClass.Pass passed",
                "SampleTestClass.Skip skipped",

                "SampleTestClass.Fail failed: 'ClassTearDown' failed!",
                "SampleTestClass.Pass failed: 'ClassTearDown' failed!",
                "SampleTestClass.Skip failed: 'ClassTearDown' failed!");

            output.ShouldHaveLifecycle(
                "ClassSetUp",
                "TestSetUp", "CaseSetUp", "Fail", "CaseTearDown", "TestTearDown",
                "TestSetUp", "CaseSetUp", "Pass", "CaseTearDown", "TestTearDown",
                "ClassTearDown");
        }

        public void ShouldSkipTestAndCaseLifecyclesWhenAllTestsAreSkipped()
        {
            var output = Run<AllSkippedTestClass, InstrumentedExecution>();

            output.ShouldHaveResults(
                "AllSkippedTestClass.SkipA skipped",
                "AllSkippedTestClass.SkipB skipped",
                "AllSkippedTestClass.SkipC skipped");

            output.ShouldHaveLifecycle("ClassSetUp", "ClassTearDown");
        }

        public void ShouldFailTestsWhenCustomParameterGenerationThrows()
        {
            var execution = new InstrumentedExecution(new BuggyParameterSource());
            var output = Run<ParameterizedSampleTestClass>(execution);

            output.ShouldHaveResults(
                "ParameterizedSampleTestClass.BoolArg failed: Exception thrown while attempting to yield input parameters for method: BoolArg",
                "ParameterizedSampleTestClass.IntArg failed: Exception thrown while attempting to yield input parameters for method: IntArg");

            //NOTE: It should be possible to limit the impact of these exceptions.
            //      This assertion is merely stating the current behavior.
            output.ShouldHaveLifecycle(
                "ClassSetUp",
                "TestSetUp",
                "TestSetUp",
                "ClassTearDown");
        }

        public void ShouldAllowRunningAllExecutionHooksMultipleTimes()
        {
            var output = Run<SampleTestClass, RunHooksTwice>();

            //NOTE: At the Case level, results are mutated in place,
            //      so here we see 2 fail results and 2 pass results,
            //      though Fail() and Pass() are each executed 4 time.
            //      This discrepancy is undesirable. This assertion is
            //      merely stating the current behavior.

            output.ShouldHaveResults(
                "SampleTestClass.Fail failed: 'Fail' failed!",
                "SampleTestClass.Fail failed: 'Fail' failed!",
                "SampleTestClass.Pass passed",
                "SampleTestClass.Pass passed",
                "SampleTestClass.Skip skipped",

                "SampleTestClass.Fail failed: 'Fail' failed!",
                "SampleTestClass.Fail failed: 'Fail' failed!",
                "SampleTestClass.Pass passed",
                "SampleTestClass.Pass passed",
                "SampleTestClass.Skip skipped");

            output.ShouldHaveLifecycle(
                "Fail", "Fail", "Fail", "Fail",
                "Pass", "Pass", "Pass", "Pass",
                "Fail", "Fail", "Fail", "Fail",
                "Pass", "Pass", "Pass", "Pass");
        }

        public void ShouldSkipAllTestsWhenShortCircuitingClassExecution()
        {
            var output = Run<SampleTestClass, ShortCircuitClassExecution>();

            output.ShouldHaveResults(
                "SampleTestClass.Fail skipped",
                "SampleTestClass.Pass skipped",
                "SampleTestClass.Skip skipped");

            output.ShouldHaveLifecycle();
        }

        public void ShouldSkipAllTestsWhenShortCircuitingTestExecution()
        {
            var output = Run<SampleTestClass, ShortCircuitTestExection>();

            output.ShouldHaveResults(
                "SampleTestClass.Fail skipped",
                "SampleTestClass.Pass skipped",
                "SampleTestClass.Skip skipped");

            output.ShouldHaveLifecycle();
        }

        public void ShouldSkipAllCasesWhenShortCircuitingCaseExecution()
        {
            var output = Run<SampleTestClass, ShortCircuitCaseExection>();

            output.ShouldHaveResults(
                "SampleTestClass.Fail skipped",
                "SampleTestClass.Pass skipped",
                "SampleTestClass.Skip skipped");

            output.ShouldHaveLifecycle();
        }
    }
}