namespace Fixie.Tests;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Utility;

public class TestClassLifecycleTests : InstrumentedExecutionTests
{
    class FirstTestClass
    {
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
    }

    class SecondTestClass
    {
        public void SecondPass()
        {
            WhereAmI();
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

    class InstrumentedExecution : IExecution
    {
        public async Task Run(TestSuite testSuite)
        {
            AssemblySetUp();

            foreach (var testClass in testSuite.TestClasses)
                await TestClassLifecycle(testClass);

            AssemblyTearDown();
        }

        async Task TestClassLifecycle(TestClass testClass)
        {
            try
            {
                ClassSetUp();

                foreach (var test in testClass.Tests)
                    if (!test.Name.Contains("Skip"))
                        await TestLifecycle(test);

                ClassTearDown();
            }
            catch (Exception exception)
            {
                await testClass.Fail(exception);
            }
        }

        async Task TestLifecycle(Test test)
        {
            try
            {
                TestSetUp();

                foreach (var parameters in YieldParameters(test))
                    await CaseLifecycle(test, parameters);

                TestTearDown();
            }
            catch (Exception exception)
            {
                await test.Fail(exception);
            }
        }

        static IEnumerable<object?[]> YieldParameters(Test test)
        {
            ProcessScriptedFailure();

            return FromInputAttributes(test);
        }

        static async Task CaseLifecycle(Test test, object?[] parameters)
        {
            try
            {
                CaseSetUp();
                await test.Run(parameters);
                CaseTearDown();
            }
            catch (Exception exception)
            {
                await test.Fail(parameters, exception);
            }
        }
    }

    static void AssemblySetUp() => WhereAmI();
    static void ClassSetUp() => WhereAmI();
    static void TestSetUp() => WhereAmI();
    static void CaseSetUp() => WhereAmI();
    static void CaseTearDown() => WhereAmI();
    static void TestTearDown() => WhereAmI();
    static void ClassTearDown() => WhereAmI();
    static void AssemblyTearDown() => WhereAmI();

    class ShortCircuitTestExecution : IExecution
    {
        public Task Run(TestSuite testSuite)
        {
            //Lifecycle chooses not to invoke any tests.
            //Since the tests never run, they are all
            //considered 'skipped'.
            return Task.CompletedTask;
        }
    }

    class RepeatedExecution : IExecution
    {
        public async Task Run(TestSuite testSuite)
        {
            foreach (var test in testSuite.Tests)
            {
                if (test.Name.Contains("Skip")) continue;

                for (int i = 1; i <= 3; i++)
                    foreach (var parameters in FromInputAttributes(test))
                        await test.Run(parameters);
            }
        }
    }

    class CircuitBreakingExecution : IExecution
    {
        readonly int maxFailures;

        public CircuitBreakingExecution(int maxFailures)
            => this.maxFailures = maxFailures;

        public async Task Run(TestSuite testSuite)
        {
            int failures = 0;

            foreach (var test in testSuite.Tests)
            {
                if (test.Name.Contains("Skip")) continue;

                for (int i = 1; i <= 3; i++)
                {
                    foreach (var parameters in FromInputAttributes(test))
                    {
                        var result = await test.Run(parameters);

                        if (result is Failed)
                        {
                            failures++;

                            if (failures > maxFailures)
                                return;
                        }
                    }
                }
            }
        }
    }

    public async Task ShouldRunAllTestsByDefault()
    {
        var output = await Run<FirstTestClass, SecondTestClass, DefaultExecution>();

        //NOTE: With no input parameter or skip behaviors,
        //      all test methods are attempted and with zero
        //      parameters, so Skip() is reached and Pass(int)
        //      is attempted but never reached.

        output.ShouldHaveResults(
            "FirstTestClass.Fail failed: 'Fail' failed!",
            "FirstTestClass.Pass failed: Parameter count mismatch.",
            "FirstTestClass.Skip failed: 'Skip' reached a line of code thought to be unreachable.",
            "SecondTestClass.SecondPass passed");

        output.ShouldHaveLifecycle("Fail", "Skip", "SecondPass");
    }

    public async Task ShouldSupportExecutionLifecycleHooks()
    {
        var output = await Run<FirstTestClass, SecondTestClass, InstrumentedExecution>();

        output.ShouldHaveResults(
            "FirstTestClass.Fail failed: 'Fail' failed!",
            "FirstTestClass.Pass(1) passed",
            "FirstTestClass.Pass(2) passed",
            "SecondTestClass.SecondPass passed",
            "FirstTestClass.Skip skipped: This test did not run.");

        output.ShouldHaveLifecycle(
            "AssemblySetUp",

            "ClassSetUp",
            "TestSetUp",
            "CaseSetUp", "Fail", "CaseTearDown",
            "TestTearDown",
            "TestSetUp",
            "CaseSetUp", "Pass(1)", "CaseTearDown",
            "CaseSetUp", "Pass(2)", "CaseTearDown",
            "TestTearDown",
            "ClassTearDown",

            "ClassSetUp",
            "TestSetUp",
            "CaseSetUp",
            "SecondPass",
            "CaseTearDown",
            "TestTearDown",
            "ClassTearDown",

            "AssemblyTearDown");
    }

    public async Task ShouldSupportStaticTestClassesAndMethods()
    {
        var output = await Run<InstrumentedExecution>(typeof(StaticTestClass));

        output.ShouldHaveResults(
            "StaticTestClass.Fail failed: 'Fail' failed!",
            "StaticTestClass.Pass passed",
            "StaticTestClass.Skip skipped: This test did not run.");

        output.ShouldHaveLifecycle(
            "AssemblySetUp",
            "ClassSetUp",
            "TestSetUp", "CaseSetUp", "Fail", "CaseTearDown", "TestTearDown",
            "TestSetUp", "CaseSetUp", "Pass", "CaseTearDown", "TestTearDown",
            "ClassTearDown",
            "AssemblyTearDown");
    }

    public async Task ShouldFailAllTestsWithoutHidingPrimarySkipResultsWhenAssemblySetUpThrows()
    {
        FailDuring("AssemblySetUp");

        var output = await Run<FirstTestClass, SecondTestClass, InstrumentedExecution>();

        output.ShouldHaveResults(
            "FirstTestClass.Fail failed: 'AssemblySetUp' failed!",
            "FirstTestClass.Fail skipped: This test did not run.",

            "FirstTestClass.Pass failed: 'AssemblySetUp' failed!",
            "FirstTestClass.Pass skipped: This test did not run.",

            "FirstTestClass.Skip failed: 'AssemblySetUp' failed!",
            "FirstTestClass.Skip skipped: This test did not run.",
            
            "SecondTestClass.SecondPass failed: 'AssemblySetUp' failed!",
            "SecondTestClass.SecondPass skipped: This test did not run.");

        output.ShouldHaveLifecycle("AssemblySetUp");
    }

    public async Task ShouldFailAllTestsInClassWithoutHidingPrimarySkipResultsWhenClassSetUpThrows()
    {
        FailDuring("ClassSetUp", occurrence: 1);
    
        var output = await Run<FirstTestClass, SecondTestClass, InstrumentedExecution>();
    
        output.ShouldHaveResults(
            "FirstTestClass.Fail failed: 'ClassSetUp' failed!",
            "FirstTestClass.Pass failed: 'ClassSetUp' failed!",
            "FirstTestClass.Skip failed: 'ClassSetUp' failed!",
            "SecondTestClass.SecondPass passed");
    
        output.ShouldHaveLifecycle(
            "AssemblySetUp",
            "ClassSetUp",
            "ClassSetUp", "TestSetUp", "CaseSetUp", "SecondPass", "CaseTearDown", "TestTearDown", "ClassTearDown",
            "AssemblyTearDown");
    }

    public async Task ShouldFailTestWhenTestSetUpThrows()
    {
        FailDuring("TestSetUp", occurrence: 2);

        var output = await Run<FirstTestClass, SecondTestClass, InstrumentedExecution>();

        output.ShouldHaveResults(
            "FirstTestClass.Fail failed: 'Fail' failed!",
            "FirstTestClass.Pass failed: 'TestSetUp' failed!",
            "SecondTestClass.SecondPass passed",
            "FirstTestClass.Skip skipped: This test did not run.");

        output.ShouldHaveLifecycle(
            "AssemblySetUp",
            
            "ClassSetUp",
            "TestSetUp", "CaseSetUp", "Fail", "CaseTearDown", "TestTearDown",
            "TestSetUp",
            "ClassTearDown",

            "ClassSetUp",
            "TestSetUp", "CaseSetUp", "SecondPass", "CaseTearDown", "TestTearDown",
            "ClassTearDown",

            "AssemblyTearDown");
    }

    public async Task ShouldFailTestWhenCustomParameterGenerationThrows()
    {
        FailDuring("YieldParameters", 2);

        var execution = new InstrumentedExecution();
        var output = await Run<FirstTestClass>(execution);

        output.ShouldHaveResults(
            "FirstTestClass.Fail failed: 'Fail' failed!",
            "FirstTestClass.Pass failed: 'YieldParameters' failed!",
            "FirstTestClass.Skip skipped: This test did not run.");

        output.ShouldHaveLifecycle(
            "AssemblySetUp",
            "ClassSetUp",
            "TestSetUp",
            "CaseSetUp", "Fail", "CaseTearDown",
            "TestTearDown",
            "TestSetUp",
            "ClassTearDown",
            "AssemblyTearDown");
    }

    public async Task ShouldFailCaseWhenCaseSetUpThrows()
    {
        FailDuring("CaseSetUp", occurrence: 2);

        var output = await Run<FirstTestClass, SecondTestClass, InstrumentedExecution>();

        output.ShouldHaveResults(
            "FirstTestClass.Fail failed: 'Fail' failed!",
            "FirstTestClass.Pass(1) failed: 'CaseSetUp' failed!",
            "FirstTestClass.Pass(2) passed",
            "SecondTestClass.SecondPass passed",
            "FirstTestClass.Skip skipped: This test did not run.");

        output.ShouldHaveLifecycle(
            "AssemblySetUp",

            "ClassSetUp",
            "TestSetUp",
            "CaseSetUp", "Fail", "CaseTearDown",
            "TestTearDown",
            "TestSetUp",
            "CaseSetUp",
            "CaseSetUp", "Pass(2)", "CaseTearDown",
            "TestTearDown",
            "ClassTearDown",

            "ClassSetUp",
            "TestSetUp",
            "CaseSetUp",
            "SecondPass",
            "CaseTearDown",
            "TestTearDown",
            "ClassTearDown",

            "AssemblyTearDown");
    }

    public async Task ShouldFailCaseWithoutHidingPrimaryCaseResultsWhenCaseTearDownThrows()
    {
        FailDuring("CaseTearDown");

        var output = await Run<FirstTestClass, SecondTestClass, InstrumentedExecution>();

        output.ShouldHaveResults(
            "FirstTestClass.Fail failed: 'Fail' failed!",
            "FirstTestClass.Fail failed: 'CaseTearDown' failed!",
            "FirstTestClass.Pass(1) passed",
            "FirstTestClass.Pass(1) failed: 'CaseTearDown' failed!",
            "FirstTestClass.Pass(2) passed",
            "FirstTestClass.Pass(2) failed: 'CaseTearDown' failed!",
            "SecondTestClass.SecondPass passed",
            "SecondTestClass.SecondPass failed: 'CaseTearDown' failed!",
            "FirstTestClass.Skip skipped: This test did not run.");

        output.ShouldHaveLifecycle(
            "AssemblySetUp",

            "ClassSetUp",
            "TestSetUp",
            "CaseSetUp", "Fail", "CaseTearDown",
            "TestTearDown",
            "TestSetUp",
            "CaseSetUp", "Pass(1)", "CaseTearDown",
            "CaseSetUp", "Pass(2)", "CaseTearDown",
            "TestTearDown",
            "ClassTearDown",

            "ClassSetUp",
            "TestSetUp",
            "CaseSetUp",
            "SecondPass",
            "CaseTearDown",
            "TestTearDown",
            "ClassTearDown",

            "AssemblyTearDown");
    }

    public async Task ShouldFailTestWithoutHidingPrimaryCaseResultsWhenTestTearDownThrows()
    {
        FailDuring("TestTearDown");

        var output = await Run<FirstTestClass, SecondTestClass, InstrumentedExecution>();

        output.ShouldHaveResults(
            "FirstTestClass.Fail failed: 'Fail' failed!",
            "FirstTestClass.Fail failed: 'TestTearDown' failed!",

            "FirstTestClass.Pass(1) passed",
            "FirstTestClass.Pass(2) passed",
            "FirstTestClass.Pass failed: 'TestTearDown' failed!",

            "SecondTestClass.SecondPass passed",
            "SecondTestClass.SecondPass failed: 'TestTearDown' failed!",
            
            "FirstTestClass.Skip skipped: This test did not run.");

        output.ShouldHaveLifecycle(
            "AssemblySetUp",

            "ClassSetUp",
            "TestSetUp",
            "CaseSetUp", "Fail", "CaseTearDown",
            "TestTearDown",
            "TestSetUp",
            "CaseSetUp", "Pass(1)", "CaseTearDown",
            "CaseSetUp", "Pass(2)", "CaseTearDown",
            "TestTearDown",
            "ClassTearDown",

            "ClassSetUp",
            "TestSetUp",
            "CaseSetUp",
            "SecondPass",
            "CaseTearDown",
            "TestTearDown",
            "ClassTearDown",

            "AssemblyTearDown");
    }

    public async Task ShouldFailAllTestsInClassWithoutHidingPrimaryCaseResultsWhenClassTearDownThrows()
    {
        FailDuring("ClassTearDown", occurrence: 1);

        var output = await Run<FirstTestClass, SecondTestClass, InstrumentedExecution>();

        output.ShouldHaveResults(
            "FirstTestClass.Fail failed: 'Fail' failed!",
            "FirstTestClass.Pass(1) passed",
            "FirstTestClass.Pass(2) passed",

            "FirstTestClass.Fail failed: 'ClassTearDown' failed!",
            "FirstTestClass.Pass failed: 'ClassTearDown' failed!",
            "FirstTestClass.Skip failed: 'ClassTearDown' failed!",

            "SecondTestClass.SecondPass passed");

        output.ShouldHaveLifecycle(
            "AssemblySetUp",

            "ClassSetUp",
            "TestSetUp",
            "CaseSetUp", "Fail", "CaseTearDown",
            "TestTearDown",
            "TestSetUp",
            "CaseSetUp", "Pass(1)", "CaseTearDown",
            "CaseSetUp", "Pass(2)", "CaseTearDown",
            "TestTearDown",
            "ClassTearDown",

            "ClassSetUp",
            "TestSetUp",
            "CaseSetUp",
            "SecondPass",
            "CaseTearDown",
            "TestTearDown",
            "ClassTearDown",

            "AssemblyTearDown");
    }

    public async Task ShouldFailAllTestsWithoutHidingPrimaryCaseResultsWhenAssemblyTearDownThrows()
    {
        FailDuring("AssemblyTearDown");

        var output = await Run<FirstTestClass, SecondTestClass, InstrumentedExecution>();

        output.ShouldHaveResults(
            "FirstTestClass.Fail failed: 'Fail' failed!",
            "FirstTestClass.Pass(1) passed",
            "FirstTestClass.Pass(2) passed",

            "SecondTestClass.SecondPass passed",

            "FirstTestClass.Fail failed: 'AssemblyTearDown' failed!",
            "FirstTestClass.Pass failed: 'AssemblyTearDown' failed!",
            "FirstTestClass.Skip failed: 'AssemblyTearDown' failed!",
            "FirstTestClass.Skip skipped: This test did not run.",
            
            "SecondTestClass.SecondPass failed: 'AssemblyTearDown' failed!");

        output.ShouldHaveLifecycle(
            "AssemblySetUp",

            "ClassSetUp",
            "TestSetUp",
            "CaseSetUp", "Fail", "CaseTearDown",
            "TestTearDown",
            "TestSetUp",
            "CaseSetUp", "Pass(1)", "CaseTearDown",
            "CaseSetUp", "Pass(2)", "CaseTearDown",
            "TestTearDown",
            "ClassTearDown",

            "ClassSetUp",
            "TestSetUp",
            "CaseSetUp",
            "SecondPass",
            "CaseTearDown",
            "TestTearDown",
            "ClassTearDown",

            "AssemblyTearDown");
    }

    public async Task ShouldSkipTestLifecyclesWhenTestsAreSkipped()
    {
        var output = await Run<AllSkippedTestClass, InstrumentedExecution>();

        output.ShouldHaveResults(
            "AllSkippedTestClass.SkipA skipped: This test did not run.",
            "AllSkippedTestClass.SkipB skipped: This test did not run.",
            "AllSkippedTestClass.SkipC skipped: This test did not run.");

        output.ShouldHaveLifecycle("AssemblySetUp", "ClassSetUp", "ClassTearDown", "AssemblyTearDown");
    }

    public async Task ShouldAllowRunningTestsMultipleTimesWithDistinctResultPerInvocation()
    {
        FailDuring("Pass", occurrence: 3);

        var output = await Run<FirstTestClass, SecondTestClass, RepeatedExecution>();

        output.ShouldHaveResults(
            "FirstTestClass.Fail failed: 'Fail' failed!",
            "FirstTestClass.Fail failed: 'Fail' failed!",
            "FirstTestClass.Fail failed: 'Fail' failed!",
            
            "FirstTestClass.Pass(1) passed",
            "FirstTestClass.Pass(2) passed",
            "FirstTestClass.Pass(1) failed: 'Pass' failed!",
            "FirstTestClass.Pass(2) passed",
            "FirstTestClass.Pass(1) passed",
            "FirstTestClass.Pass(2) passed",

            "SecondTestClass.SecondPass passed",
            "SecondTestClass.SecondPass passed",
            "SecondTestClass.SecondPass passed",

            "FirstTestClass.Skip skipped: This test did not run.");

        output.ShouldHaveLifecycle(
            "Fail",
            "Fail",
            "Fail",
            "Pass(1)", "Pass(2)",
            "Pass(1)", "Pass(2)",
            "Pass(1)", "Pass(2)",
            "SecondPass",
            "SecondPass",
            "SecondPass");
    }

    public async Task ShouldAllowInspectionOfIndividualTestInvocationResultsToDriveExecutionDecisions()
    {
        FailDuring("Pass", occurrence: 3);

        var output = await Run<FirstTestClass, SecondTestClass>(new CircuitBreakingExecution(maxFailures: 3));

        output.ShouldHaveResults(
            "FirstTestClass.Fail failed: 'Fail' failed!",
            "FirstTestClass.Fail failed: 'Fail' failed!",
            "FirstTestClass.Fail failed: 'Fail' failed!",
            
            "FirstTestClass.Pass(1) passed",
            "FirstTestClass.Pass(2) passed",

            "FirstTestClass.Pass(1) failed: 'Pass' failed!", //The fourth failure stops the entire run early.

            "FirstTestClass.Skip skipped: This test did not run.",
            "SecondTestClass.SecondPass skipped: This test did not run.");

        output.ShouldHaveLifecycle(
            "Fail",
            "Fail",
            "Fail",
            "Pass(1)", "Pass(2)",
            "Pass(1)");
    }

    public async Task ShouldSkipAllTestsWhenShortCircuitingTestExecution()
    {
        var output = await Run<FirstTestClass, SecondTestClass, ShortCircuitTestExecution>();

        output.ShouldHaveResults(
            "FirstTestClass.Fail skipped: This test did not run.",
            "FirstTestClass.Pass skipped: This test did not run.",
            "FirstTestClass.Skip skipped: This test did not run.",
            "SecondTestClass.SecondPass skipped: This test did not run.");

        output.ShouldHaveLifecycle();
    }
}