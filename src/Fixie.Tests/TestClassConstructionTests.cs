namespace Fixie.Tests
{
    using System.Threading.Tasks;
    using static Utility;
    
    public class TestClassConstructionTests : InstrumentedExecutionTests
    {
        class SampleTestClass
        {
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
        }

        class AllSkippedTestClass
        {
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

        class CannotInvokeConstructorTestClass
        {
            public CannotInvokeConstructorTestClass(int argument) { }

            public void UnreachableCase()
            {
                WhereAmI();
                throw new ShouldBeUnreachableException();
            }
        }

        static bool ShouldSkip(Test test)
            => test.Name.Contains("Skip");

        class CreateInstancePerCaseImplicitly : IExecution
        {
            public async Task Run(TestSuite testSuite)
            {
                foreach (var test in testSuite.Tests)
                    if (!ShouldSkip(test))
                        foreach (var parameters in FromInputAttributes(test))
                            await test.Run(parameters);
            }
        }

        class CreateInstancePerCaseExplicitly : IExecution
        {
            public async Task Run(TestSuite testSuite)
            {
                foreach (var testClass in testSuite.TestClasses)
                {
                    var type = testClass.Type;

                    foreach (var test in testClass.Tests)
                        if (!ShouldSkip(test))
                            foreach (var parameters in FromInputAttributes(test))
                            {
                                if (test.Method.IsStatic)
                                    await test.Run(parameters);
                                else
                                    await test.Run(testClass.Construct(), parameters);
                            }
                }
            }
        }

        class CreateInstancePerClassExplicitly : IExecution
        {
            public async Task Run(TestSuite testSuite)
            {
                foreach (var testClass in testSuite.TestClasses)
                {
                    var type = testClass.Type;
                    var instance = type.IsStatic() ? null : testClass.Construct();

                    foreach (var test in testClass.Tests)
                        if (!ShouldSkip(test))
                            foreach (var parameters in FromInputAttributes(test))
                            {
                                if (test.Method.IsStatic)
                                    await test.Run(parameters);
                                else
                                    await test.Run(instance!, parameters);
                            }
                }
            }
        }

        public async Task ShouldConstructPerCaseByDefault()
        {
            //NOTE: With no input parameter or skip behaviors,
            //      all test methods are attempted once and with zero
            //      parameters, so Skip() is reached and Pass(int)
            //      is attempted once but never reached.

            var output = await Run<SampleTestClass, DefaultExecution>();

            output.ShouldHaveResults(
                "SampleTestClass.Fail failed: 'Fail' failed!",
                "SampleTestClass.Pass failed: Parameter count mismatch.",
                "SampleTestClass.Skip failed: 'Skip' reached a line of code thought to be unreachable.");

            output.ShouldHaveLifecycle(
                ".ctor", "Fail",
                ".ctor",
                ".ctor", "Skip");
        }

        public async Task ShouldAllowConstructingPerCaseImplicitly()
        {
            var output = await Run<SampleTestClass, CreateInstancePerCaseImplicitly>();

            output.ShouldHaveResults(
                "SampleTestClass.Fail failed: 'Fail' failed!",
                "SampleTestClass.Pass(1) passed",
                "SampleTestClass.Pass(2) passed",
                "SampleTestClass.Skip skipped: This test did not run.");

            output.ShouldHaveLifecycle(
                ".ctor", "Fail",
                ".ctor", "Pass(1)",
                ".ctor", "Pass(2)");
        }

        public async Task ShouldAllowConstructingPerCaseExplicitly()
        {
            var output = await Run<SampleTestClass, CreateInstancePerCaseExplicitly>();

            output.ShouldHaveResults(
                "SampleTestClass.Fail failed: 'Fail' failed!",
                "SampleTestClass.Pass(1) passed",
                "SampleTestClass.Pass(2) passed",
                "SampleTestClass.Skip skipped: This test did not run.");

            output.ShouldHaveLifecycle(
                ".ctor", "Fail",
                ".ctor", "Pass(1)",
                ".ctor", "Pass(2)");
        }

        public async Task ShouldAllowConstructingPerClassExplicitly()
        {
            var output = await Run<SampleTestClass, CreateInstancePerClassExplicitly>();

            output.ShouldHaveResults(
                "SampleTestClass.Fail failed: 'Fail' failed!",
                "SampleTestClass.Pass(1) passed",
                "SampleTestClass.Pass(2) passed",
                "SampleTestClass.Skip skipped: This test did not run.");

            output.ShouldHaveLifecycle(
                ".ctor",
                "Fail",
                "Pass(1)",
                "Pass(2)");
        }

        public async Task ShouldFailCaseInAbsenseOfPrimaryCaseResultWhenConstructingPerCaseImplicitlyAndConstructorThrows()
        {
            FailDuring(".ctor");
            
            var output = await Run<SampleTestClass, CreateInstancePerCaseImplicitly>();

            output.ShouldHaveResults(
                "SampleTestClass.Fail failed: '.ctor' failed!",
                "SampleTestClass.Pass(1) failed: '.ctor' failed!",
                "SampleTestClass.Pass(2) failed: '.ctor' failed!",
                "SampleTestClass.Skip skipped: This test did not run.");

            output.ShouldHaveLifecycle(
                ".ctor",
                ".ctor",
                ".ctor");
        }

        public async Task ShouldFailAllTestsWithoutHidingPrimarySkipResultsWhenConstructingPerCaseExplicitlyAndConstructorThrows()
        {
            FailDuring(".ctor");
            
            var output = await Run<SampleTestClass, CreateInstancePerCaseExplicitly>();

            output.ShouldHaveResults(
                "SampleTestClass.Fail failed: '.ctor' failed!",
                "SampleTestClass.Fail skipped: This test did not run.",
                "SampleTestClass.Pass failed: '.ctor' failed!",
                "SampleTestClass.Pass skipped: This test did not run.",
                "SampleTestClass.Skip failed: '.ctor' failed!",
                "SampleTestClass.Skip skipped: This test did not run.");

            output.ShouldHaveLifecycle(".ctor");
        }

        public async Task ShouldFailAllTestsWithoutHidingPrimarySkipResultsWhenConstructingPerClassExplicitlyAndConstructorThrows()
        {
            FailDuring(".ctor");

            var output = await Run<SampleTestClass, CreateInstancePerClassExplicitly>();

            output.ShouldHaveResults(
                "SampleTestClass.Fail failed: '.ctor' failed!",
                "SampleTestClass.Fail skipped: This test did not run.",
                "SampleTestClass.Pass failed: '.ctor' failed!",
                "SampleTestClass.Pass skipped: This test did not run.",
                "SampleTestClass.Skip failed: '.ctor' failed!",
                "SampleTestClass.Skip skipped: This test did not run."
            );

            output.ShouldHaveLifecycle(".ctor");
        }

        public async Task ShouldBypassConstructionWhenConstructingPerCaseImplicitlyAndAllCasesAreSkipped()
        {
            var output = await Run<AllSkippedTestClass, CreateInstancePerCaseImplicitly>();

            output.ShouldHaveResults(
                "AllSkippedTestClass.SkipA skipped: This test did not run.",
                "AllSkippedTestClass.SkipB skipped: This test did not run.",
                "AllSkippedTestClass.SkipC skipped: This test did not run.");

            output.ShouldHaveLifecycle();
        }

        public async Task ShouldBypassConstructionWhenConstructingPerCaseExplicitlyAndAllCasesAreSkipped()
        {
            var output = await Run<AllSkippedTestClass, CreateInstancePerCaseExplicitly>();

            output.ShouldHaveResults(
                "AllSkippedTestClass.SkipA skipped: This test did not run.",
                "AllSkippedTestClass.SkipB skipped: This test did not run.",
                "AllSkippedTestClass.SkipC skipped: This test did not run.");

            output.ShouldHaveLifecycle();
        }

        public async Task ShouldNotBypassConstructionWhenConstructingPerClassExplicitlyAndAllCasesAreSkipped()
        {
            var output = await Run<AllSkippedTestClass, CreateInstancePerClassExplicitly>();

            output.ShouldHaveResults(
                "AllSkippedTestClass.SkipA skipped: This test did not run.",
                "AllSkippedTestClass.SkipB skipped: This test did not run.",
                "AllSkippedTestClass.SkipC skipped: This test did not run.");

            output.ShouldHaveLifecycle(".ctor");
        }

        public async Task ShouldBypassConstructionAttemptsWhenTestMethodsAreStatic()
        {
            var output = await Run<DefaultExecution>(typeof(StaticTestClass));

            output.ShouldHaveResults(
                "StaticTestClass.Fail failed: 'Fail' failed!",
                "StaticTestClass.Pass passed",
                "StaticTestClass.Skip failed: 'Skip' reached a line of code thought to be unreachable."
            );

            output.ShouldHaveLifecycle("Fail", "Pass", "Skip");


            output = await Run<CreateInstancePerCaseImplicitly>(typeof(StaticTestClass));

            output.ShouldHaveResults(
                "StaticTestClass.Fail failed: 'Fail' failed!",
                "StaticTestClass.Pass passed",
                "StaticTestClass.Skip skipped: This test did not run."
            );

            output.ShouldHaveLifecycle("Fail", "Pass");


            output = await Run<CreateInstancePerCaseExplicitly>(typeof(StaticTestClass));

            output.ShouldHaveResults(
                "StaticTestClass.Fail failed: 'Fail' failed!",
                "StaticTestClass.Pass passed",
                "StaticTestClass.Skip skipped: This test did not run."
            );

            output.ShouldHaveLifecycle("Fail", "Pass");


            output = await Run<CreateInstancePerClassExplicitly>(typeof(StaticTestClass));

            output.ShouldHaveResults(
                "StaticTestClass.Fail failed: 'Fail' failed!",
                "StaticTestClass.Pass passed",
                "StaticTestClass.Skip skipped: This test did not run."
            );

            output.ShouldHaveLifecycle("Fail", "Pass");
        }

        public async Task ShouldFailWhenTestClassConstructorCannotBeInvoked()
        {
            var output = await Run<CannotInvokeConstructorTestClass, DefaultExecution>();

            output.ShouldHaveResults(
                "CannotInvokeConstructorTestClass.UnreachableCase failed: " +
                "Cannot dynamically create an instance " +
                $"of type '{FullName<CannotInvokeConstructorTestClass>()}'. " +
                "Reason: No parameterless constructor defined.");

            output.ShouldHaveLifecycle();
        }
    }
}