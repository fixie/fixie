namespace Fixie.Tests
{
    using System;
    using System.Threading.Tasks;
    using Fixie.Internal;

    public class TestClassConstructionTests : InstrumentedExecutionTests
    {
        class SampleTestClass : IAsyncDisposable, IDisposable
        {
            bool asyncDisposed;
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

            public ValueTask DisposeAsync()
            {
                if (asyncDisposed)
                    throw new ShouldBeUnreachableException();
                asyncDisposed = true;
            
                WhereAmI();
            
                return default;
            }
        }

        class AllSkippedTestClass : IAsyncDisposable, IDisposable
        {
            bool asyncDisposed;
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

            public ValueTask DisposeAsync()
            {
                if (asyncDisposed)
                    throw new ShouldBeUnreachableException();
                asyncDisposed = true;
            
                WhereAmI();
            
                return default;
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

        static bool ShouldSkip(TestMethod test)
            => test.Method.Name.Contains("Skip");

        class CreateInstancePerCase : Execution
        {
            public async Task RunAsync(TestClass testClass)
            {
                foreach (var test in testClass.Tests)
                    if (!ShouldSkip(test))
                        await test.RunAsync(Utility.UsingInputAttributes);
            }
        }

        class CreateInstancePerClass : Execution
        {
            public async Task RunAsync(TestClass testClass)
            {
                var type = testClass.Type;
                var instance = type.IsStatic() ? null : testClass.Construct();

                foreach (var test in testClass.Tests)
                    if (!ShouldSkip(test))
                        await test.RunAsync(Utility.UsingInputAttributes, instance);

                await instance.DisposeIfApplicableAsync();
            }
        }

        public async Task ShouldConstructPerCaseByDefault()
        {
            //NOTE: With no input parameter or skip behaviors,
            //      all test methods are attempted once and with zero
            //      parameters, so Skip() is reached and Pass(int)
            //      is attempted once but never reached.

            var output = await RunAsync<SampleTestClass, DefaultExecution>();

            output.ShouldHaveResults(
                "SampleTestClass.Fail failed: 'Fail' failed!",
                "SampleTestClass.Pass failed: Parameter count mismatch.",
                "SampleTestClass.Skip failed: 'Skip' reached a line of code thought to be unreachable.");

            output.ShouldHaveLifecycle(
                ".ctor", "Fail", "DisposeAsync", "Dispose",
                ".ctor", "DisposeAsync", "Dispose",
                ".ctor", "Skip", "DisposeAsync", "Dispose");
        }

        public async Task ShouldAllowConstructingPerCase()
        {
            var output = await RunAsync<SampleTestClass, CreateInstancePerCase>();

            output.ShouldHaveResults(
                "SampleTestClass.Fail failed: 'Fail' failed!",
                "SampleTestClass.Pass(1) passed",
                "SampleTestClass.Pass(2) passed",
                "SampleTestClass.Skip skipped: This test did not run.");

            output.ShouldHaveLifecycle(
                ".ctor", "Fail", "DisposeAsync", "Dispose",
                ".ctor", "Pass(1)", "DisposeAsync","Dispose",
                ".ctor", "Pass(2)", "DisposeAsync","Dispose");
        }

        public async Task ShouldFailCaseInAbsenseOfPrimaryCaseResultAndProceedWithFailureInspectionWhenConstructingPerCaseAndConstructorThrows()
        {
            FailDuring(".ctor");
            
            var output = await RunAsync<SampleTestClass, CreateInstancePerCase>();

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

        public async Task ShouldFailCaseWithoutHidingPrimaryFailuresWhenConstructingPerCaseAndDisposeThrows()
        {
            FailDuring("Dispose");

            var output = await RunAsync<SampleTestClass, CreateInstancePerCase>();

            output.ShouldHaveResults(
                "SampleTestClass.Fail failed: 'Fail' failed!",
                "SampleTestClass.Fail failed: 'Dispose' failed!",
                "SampleTestClass.Pass(1) failed: 'Dispose' failed!",
                "SampleTestClass.Pass(2) failed: 'Dispose' failed!",
                "SampleTestClass.Skip skipped: This test did not run.");

            output.ShouldHaveLifecycle(
                ".ctor", "Fail", "DisposeAsync", "Dispose",
                ".ctor", "Pass(1)", "DisposeAsync", "Dispose",
                ".ctor", "Pass(2)", "DisposeAsync", "Dispose");
        }

        public async Task ShouldFailCaseWithoutHidingPrimaryFailuresAndShortCircuitSynchronousDisposeWhenConstructingPerCaseAndDisposeAsyncThrows()
        {
            FailDuring("DisposeAsync");

            var output = await RunAsync<SampleTestClass, CreateInstancePerCase>();

            output.ShouldHaveResults(
                "SampleTestClass.Fail failed: 'Fail' failed!",
                "SampleTestClass.Fail failed: 'DisposeAsync' failed!",
                "SampleTestClass.Pass(1) failed: 'DisposeAsync' failed!",
                "SampleTestClass.Pass(2) failed: 'DisposeAsync' failed!",
                "SampleTestClass.Skip skipped: This test did not run.");

            output.ShouldHaveLifecycle(
                ".ctor", "Fail", "DisposeAsync",
                ".ctor", "Pass(1)", "DisposeAsync",
                ".ctor", "Pass(2)", "DisposeAsync");
        }

        public async Task ShouldAllowConstructingPerClass()
        {
            var output = await RunAsync<SampleTestClass, CreateInstancePerClass>();

            output.ShouldHaveResults(
                "SampleTestClass.Fail failed: 'Fail' failed!",
                "SampleTestClass.Pass(1) passed",
                "SampleTestClass.Pass(2) passed",
                "SampleTestClass.Skip skipped: This test did not run.");

            output.ShouldHaveLifecycle(
                ".ctor",
                "Fail",
                "Pass(1)",
                "Pass(2)",
                "DisposeAsync",
                "Dispose");
        }

        public async Task ShouldFailAllTestsWithoutHidingPrimarySkipResultsWhenConstructingPerClassAndConstructorThrows()
        {
            FailDuring(".ctor");

            var output = await RunAsync<SampleTestClass, CreateInstancePerClass>();

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

        public async Task ShouldFailAllTestsWithoutHidingPrimaryCaseResultsWhenConstructingPerClassAndDisposeThrows()
        {
            FailDuring("Dispose");

            var output = await RunAsync<SampleTestClass, CreateInstancePerClass>();

            output.ShouldHaveResults(
                "SampleTestClass.Fail failed: 'Fail' failed!",
                "SampleTestClass.Pass(1) passed",
                "SampleTestClass.Pass(2) passed",
                "SampleTestClass.Fail failed: 'Dispose' failed!",
                "SampleTestClass.Pass failed: 'Dispose' failed!",
                "SampleTestClass.Skip failed: 'Dispose' failed!",
                "SampleTestClass.Skip skipped: This test did not run.");

            output.ShouldHaveLifecycle(
                ".ctor",
                "Fail",
                "Pass(1)",
                "Pass(2)",
                "DisposeAsync",
                "Dispose");
        }

        public async Task ShouldFailAllTestsWithoutHidingPrimaryCaseResultsAndShortCircuitSynchronousDisposeWhenConstructingPerClassAndDisposeAsyncThrows()
        {
            FailDuring("DisposeAsync");

            var output = await RunAsync<SampleTestClass, CreateInstancePerClass>();

            output.ShouldHaveResults(
                "SampleTestClass.Fail failed: 'Fail' failed!",
                "SampleTestClass.Pass(1) passed",
                "SampleTestClass.Pass(2) passed",
                "SampleTestClass.Fail failed: 'DisposeAsync' failed!",
                "SampleTestClass.Pass failed: 'DisposeAsync' failed!",
                "SampleTestClass.Skip failed: 'DisposeAsync' failed!",
                "SampleTestClass.Skip skipped: This test did not run.");

            output.ShouldHaveLifecycle(
                ".ctor",
                "Fail",
                "Pass(1)",
                "Pass(2)",
                "DisposeAsync");
        }

        public async Task ShouldBypassConstructionWhenConstructingPerCaseAndAllCasesAreSkipped()
        {
            var output = await RunAsync<AllSkippedTestClass, CreateInstancePerCase>();

            output.ShouldHaveResults(
                "AllSkippedTestClass.SkipA skipped: This test did not run.",
                "AllSkippedTestClass.SkipB skipped: This test did not run.",
                "AllSkippedTestClass.SkipC skipped: This test did not run.");

            output.ShouldHaveLifecycle();
        }

        public async Task ShouldNotBypassConstructionWhenConstructingPerClassAndAllCasesAreSkipped()
        {
            var output = await RunAsync<AllSkippedTestClass, CreateInstancePerClass>();

            output.ShouldHaveResults(
                "AllSkippedTestClass.SkipA skipped: This test did not run.",
                "AllSkippedTestClass.SkipB skipped: This test did not run.",
                "AllSkippedTestClass.SkipC skipped: This test did not run.");

            output.ShouldHaveLifecycle(".ctor", "DisposeAsync", "Dispose");
        }

        public async Task ShouldBypassConstructionAttemptsWhenTestMethodsAreStatic()
        {
            var output = await RunAsync<DefaultExecution>(typeof(StaticTestClass));

            output.ShouldHaveResults(
                "StaticTestClass.Fail failed: 'Fail' failed!",
                "StaticTestClass.Pass passed",
                "StaticTestClass.Skip failed: 'Skip' reached a line of code thought to be unreachable."
            );

            output.ShouldHaveLifecycle("Fail", "Pass", "Skip");


            output = await RunAsync<CreateInstancePerCase>(typeof(StaticTestClass));

            output.ShouldHaveResults(
                "StaticTestClass.Fail failed: 'Fail' failed!",
                "StaticTestClass.Pass passed",
                "StaticTestClass.Skip skipped: This test did not run."
            );

            output.ShouldHaveLifecycle("Fail", "Pass");

            output = await RunAsync<CreateInstancePerClass>(typeof(StaticTestClass));

            output.ShouldHaveResults(
                "StaticTestClass.Fail failed: 'Fail' failed!",
                "StaticTestClass.Pass passed",
                "StaticTestClass.Skip skipped: This test did not run."
            );

            output.ShouldHaveLifecycle("Fail", "Pass");
        }
    }
}