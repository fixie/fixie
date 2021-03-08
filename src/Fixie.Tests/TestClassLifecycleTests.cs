namespace Fixie.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Fixie.Internal;
    using Fixie.Reports;

    public class TestClassLifecycleTests : InstrumentedExecutionTests
    {
        class SampleTestClass
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

        class InstrumentedExecution : Execution
        {
            readonly ParameterSource parameterSource;

            public InstrumentedExecution()
                : this(Utility.UsingInputAttributes) { }

            public InstrumentedExecution(ParameterSource parameterSource)
                => this.parameterSource = parameterSource;

            public async Task RunAsync(TestAssembly testAssembly)
            {
                AssemblySetUp();

                foreach (var testClass in testAssembly.TestClasses)
                {
                    ClassSetUp();

                    foreach (var test in testClass.Tests)
                        if (!test.Method.Name.Contains("Skip"))
                            await TestLifecycleAsync(test);

                    ClassTearDown();
                }

                AssemblyTearDown();
            }

            async Task TestLifecycleAsync(TestMethod test)
            {
                try
                {
                    TestSetUp();

                    var cases = test.HasParameters
                        ? parameterSource(test.Method)
                        : InvokeOnceWithZeroParameters;

                    foreach (var parameters in cases)
                        await CaseLifecycleAsync(test, parameters);

                    TestTearDown();
                }
                catch (Exception exception)
                {
                    await test.FailAsync(exception);
                }
            }

            static async Task CaseLifecycleAsync(TestMethod test, object?[] parameters)
            {
                try
                {
                    CaseSetUp();
                    await test.RunAsync(parameters);
                    CaseTearDown();
                }
                catch (Exception exception)
                {
                    await test.FailAsync(parameters, exception);
                }
            }

            static readonly object[] EmptyParameters = {};
            static readonly object[][] InvokeOnceWithZeroParameters = { EmptyParameters };
        }

        static void AssemblySetUp() => WhereAmI();
        static void ClassSetUp() => WhereAmI();
        static void TestSetUp() => WhereAmI();
        static void CaseSetUp() => WhereAmI();
        static void CaseTearDown() => WhereAmI();
        static void TestTearDown() => WhereAmI();
        static void ClassTearDown() => WhereAmI();
        static void AssemblyTearDown() => WhereAmI();

        class ShortCircuitTestExecution : Execution
        {
            public Task RunAsync(TestAssembly testAssembly)
            {
                //Lifecycle chooses not to invoke any tests
                //Since the tests never run, they are all
                //considered 'skipped'.
                return Task.CompletedTask;
            }
        }

        class RepeatedExecution : Execution
        {
            public async Task RunAsync(TestAssembly testAssembly)
            {
                foreach (var testClass in testAssembly.TestClasses)
                {
                    foreach (var test in testClass.Tests)
                    {
                        if (test.Method.Name.Contains("Skip")) continue;

                        for (int i = 1; i <= 3; i++)
                            await test.RunAsync(Utility.UsingInputAttributes);
                    }
                }
            }
        }

        class CircuitBreakingExecution : Execution
        {
            readonly int maxFailures;

            public CircuitBreakingExecution(int maxFailures)
                => this.maxFailures = maxFailures;

            public async Task RunAsync(TestAssembly testAssembly)
            {
                int failures = 0;

                foreach (var testClass in testAssembly.TestClasses)
                {
                    foreach (var test in testClass.Tests)
                    {
                        if (test.Method.Name.Contains("Skip")) continue;

                        for (int i = 1; i <= 3; i++)
                        {
                            foreach (var parameters in Cases(test))
                            {
                                var result = await test.RunAsync(parameters);

                                if (result is CaseFailed)
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

            static IEnumerable<object?[]> Cases(TestMethod test)
            {
                if (test.HasParameters)
                {
                    foreach (var parameters in Utility.UsingInputAttributes(test.Method))
                        yield return parameters;
                }
                else
                {
                    yield return EmptyParameters;
                }
            }

            static readonly object[] EmptyParameters = {};
        }

        public async Task ShouldRunAllTestsByDefault()
        {
            var output = await RunAsync<SampleTestClass, DefaultExecution>();

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

        public async Task ShouldSupportExecutionLifecycleHooks()
        {
            var output = await RunAsync<SampleTestClass, InstrumentedExecution>();

            output.ShouldHaveResults(
                "SampleTestClass.Fail failed: 'Fail' failed!",
                "SampleTestClass.Pass(1) passed",
                "SampleTestClass.Pass(2) passed",
                "SampleTestClass.Skip skipped: This test did not run.");

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
                "AssemblyTearDown");
        }

        public async Task ShouldSupportStaticTestClassesAndMethods()
        {
            var output = await RunAsync<InstrumentedExecution>(typeof(StaticTestClass));

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

            var output = await RunAsync<SampleTestClass, InstrumentedExecution>();

            output.ShouldHaveResults(
                "SampleTestClass.Fail failed: 'AssemblySetUp' failed!",
                "SampleTestClass.Fail skipped: This test did not run.",

                "SampleTestClass.Pass failed: 'AssemblySetUp' failed!",
                "SampleTestClass.Pass skipped: This test did not run.",

                "SampleTestClass.Skip failed: 'AssemblySetUp' failed!",
                "SampleTestClass.Skip skipped: This test did not run.");

            output.ShouldHaveLifecycle("AssemblySetUp");
        }

        public async Task ShouldFailAllTestsWithoutHidingPrimarySkipResultsWhenClassSetUpThrows()
        {
            FailDuring("ClassSetUp");
        
            var output = await RunAsync<SampleTestClass, InstrumentedExecution>();
        
            output.ShouldHaveResults(
                "SampleTestClass.Fail failed: 'ClassSetUp' failed!",
                "SampleTestClass.Fail skipped: This test did not run.",
                
                "SampleTestClass.Pass failed: 'ClassSetUp' failed!",
                "SampleTestClass.Pass skipped: This test did not run.",
                
                "SampleTestClass.Skip failed: 'ClassSetUp' failed!",
                "SampleTestClass.Skip skipped: This test did not run.");
        
            output.ShouldHaveLifecycle("AssemblySetUp", "ClassSetUp");
        }

        public async Task ShouldFailTestWhenTestSetUpThrows()
        {
            FailDuring("TestSetUp", occurrence: 2);

            var output = await RunAsync<SampleTestClass, InstrumentedExecution>();

            output.ShouldHaveResults(
                "SampleTestClass.Fail failed: 'Fail' failed!",
                "SampleTestClass.Pass failed: 'TestSetUp' failed!",
                "SampleTestClass.Skip skipped: This test did not run.");

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

        public async Task ShouldFailTestWhenCustomParameterGenerationThrows()
        {
            var execution = new InstrumentedExecution(method =>
                throw new Exception("Failed to yield input parameters."));
            var output = await RunAsync<SampleTestClass>(execution);

            output.ShouldHaveResults(
                "SampleTestClass.Fail failed: 'Fail' failed!",
                "SampleTestClass.Pass failed: Failed to yield input parameters.",
                "SampleTestClass.Skip skipped: This test did not run.");

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

        public async Task ShouldFailTestWhenCaseSetUpThrows()
        {
            FailDuring("CaseSetUp", occurrence: 2);

            var output = await RunAsync<SampleTestClass, InstrumentedExecution>();

            output.ShouldHaveResults(
                "SampleTestClass.Fail failed: 'Fail' failed!",
                "SampleTestClass.Pass(1) failed: 'CaseSetUp' failed!",
                "SampleTestClass.Pass(2) passed",
                "SampleTestClass.Skip skipped: This test did not run.");

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
                "AssemblyTearDown");
        }

        public async Task ShouldFailTestWithoutHidingPrimaryCaseResultsWhenCaseTearDownThrows()
        {
            FailDuring("CaseTearDown");

            var output = await RunAsync<SampleTestClass, InstrumentedExecution>();

            output.ShouldHaveResults(
                "SampleTestClass.Fail failed: 'Fail' failed!",
                "SampleTestClass.Fail failed: 'CaseTearDown' failed!",
                "SampleTestClass.Pass(1) passed",
                "SampleTestClass.Pass(1) failed: 'CaseTearDown' failed!",
                "SampleTestClass.Pass(2) passed",
                "SampleTestClass.Pass(2) failed: 'CaseTearDown' failed!",
                "SampleTestClass.Skip skipped: This test did not run.");

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
                "AssemblyTearDown");
        }

        public async Task ShouldFailTestWithoutHidingPrimaryCaseResultsWhenTestTearDownThrows()
        {
            FailDuring("TestTearDown");

            var output = await RunAsync<SampleTestClass, InstrumentedExecution>();

            output.ShouldHaveResults(
                "SampleTestClass.Fail failed: 'Fail' failed!",
                "SampleTestClass.Fail failed: 'TestTearDown' failed!",

                "SampleTestClass.Pass(1) passed",
                "SampleTestClass.Pass(2) passed",
                "SampleTestClass.Pass failed: 'TestTearDown' failed!",
                
                "SampleTestClass.Skip skipped: This test did not run.");

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
                "AssemblyTearDown");
        }

        public async Task ShouldFailAllTestsWithoutHidingPrimaryCaseResultsWhenClassTearDownThrows()
        {
            FailDuring("ClassTearDown");

            var output = await RunAsync<SampleTestClass, InstrumentedExecution>();

            output.ShouldHaveResults(
                "SampleTestClass.Fail failed: 'Fail' failed!",
                "SampleTestClass.Pass(1) passed",
                "SampleTestClass.Pass(2) passed",

                "SampleTestClass.Fail failed: 'ClassTearDown' failed!",
                "SampleTestClass.Pass failed: 'ClassTearDown' failed!",
                "SampleTestClass.Skip failed: 'ClassTearDown' failed!",
                "SampleTestClass.Skip skipped: This test did not run.");

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
                "ClassTearDown");
        }

        public async Task ShouldFailAllTestsWithoutHidingPrimaryCaseResultsWhenAssemblyTearDownThrows()
        {
            FailDuring("AssemblyTearDown");

            var output = await RunAsync<SampleTestClass, InstrumentedExecution>();

            output.ShouldHaveResults(
                "SampleTestClass.Fail failed: 'Fail' failed!",
                "SampleTestClass.Pass(1) passed",
                "SampleTestClass.Pass(2) passed",

                "SampleTestClass.Fail failed: 'AssemblyTearDown' failed!",
                "SampleTestClass.Pass failed: 'AssemblyTearDown' failed!",
                "SampleTestClass.Skip failed: 'AssemblyTearDown' failed!",
                "SampleTestClass.Skip skipped: This test did not run.");

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
                "AssemblyTearDown");
        }

        public async Task ShouldSkipTestLifecyclesWhenAllTestsAreSkipped()
        {
            var output = await RunAsync<AllSkippedTestClass, InstrumentedExecution>();

            output.ShouldHaveResults(
                "AllSkippedTestClass.SkipA skipped: This test did not run.",
                "AllSkippedTestClass.SkipB skipped: This test did not run.",
                "AllSkippedTestClass.SkipC skipped: This test did not run.");

            output.ShouldHaveLifecycle("AssemblySetUp", "ClassSetUp", "ClassTearDown", "AssemblyTearDown");
        }

        public async Task ShouldAllowRunningTestsMultipleTimesWithDistinctResultPerInvocation()
        {
            FailDuring("Pass", occurrence: 3);

            var output = await RunAsync<SampleTestClass, RepeatedExecution>();

            output.ShouldHaveResults(
                "SampleTestClass.Fail failed: 'Fail' failed!",
                "SampleTestClass.Fail failed: 'Fail' failed!",
                "SampleTestClass.Fail failed: 'Fail' failed!",
                
                "SampleTestClass.Pass(1) passed",
                "SampleTestClass.Pass(2) passed",

                "SampleTestClass.Pass(1) failed: 'Pass' failed!",
                "SampleTestClass.Pass(2) passed",

                "SampleTestClass.Pass(1) passed",
                "SampleTestClass.Pass(2) passed",

                "SampleTestClass.Skip skipped: This test did not run.");

            output.ShouldHaveLifecycle(
                "Fail",
                "Fail",
                "Fail",
                "Pass(1)", "Pass(2)",
                "Pass(1)", "Pass(2)",
                "Pass(1)", "Pass(2)");
        }

        public async Task ShouldAllowInspectionOfIndividualTestInvocationResultsToDriveExecutionDecisions()
        {
            FailDuring("Pass", occurrence: 3);

            var output = await RunAsync<SampleTestClass>(new CircuitBreakingExecution(maxFailures: 3));

            output.ShouldHaveResults(
                "SampleTestClass.Fail failed: 'Fail' failed!",
                "SampleTestClass.Fail failed: 'Fail' failed!",
                "SampleTestClass.Fail failed: 'Fail' failed!",
                
                "SampleTestClass.Pass(1) passed",
                "SampleTestClass.Pass(2) passed",

                "SampleTestClass.Pass(1) failed: 'Pass' failed!", //The fourth failure stops the run early.

                "SampleTestClass.Skip skipped: This test did not run.");

            output.ShouldHaveLifecycle(
                "Fail",
                "Fail",
                "Fail",
                "Pass(1)", "Pass(2)",
                "Pass(1)");
        }

        public async Task ShouldSkipAllTestsWhenShortCircuitingTestExecution()
        {
            var output = await RunAsync<SampleTestClass, ShortCircuitTestExecution>();

            output.ShouldHaveResults(
                "SampleTestClass.Fail skipped: This test did not run.",
                "SampleTestClass.Pass skipped: This test did not run.",
                "SampleTestClass.Skip skipped: This test did not run.");

            output.ShouldHaveLifecycle();
        }
    }
}