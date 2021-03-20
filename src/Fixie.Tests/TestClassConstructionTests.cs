namespace Fixie.Tests
{
    using System;
    using System.Threading.Tasks;
    using Fixie.Internal;
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

        class DisposableSampleTestClass : IDisposable
        {
            bool disposed;

            public DisposableSampleTestClass()
            {
                WhereAmI();
            }

            public void Fail()
            {
                WhereAmI();
                throw new FailureException();
            }

            public void Pass()
            {
                WhereAmI();
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

        class AsyncDisposableSampleTestClass : IAsyncDisposable
        {
            bool asyncDisposed;

            public AsyncDisposableSampleTestClass()
            {
                WhereAmI();
            }

            public void Fail()
            {
                WhereAmI();
                throw new FailureException();
            }

            public void Pass()
            {
                WhereAmI();
            }

            public void Skip()
            {
                WhereAmI();
                throw new ShouldBeUnreachableException();
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

        static bool ShouldSkip(Test test)
            => test.Method.Name.Contains("Skip");

        class CreateInstancePerCase : Execution
        {
            public async Task RunAsync(TestAssembly testAssembly)
            {
                foreach (var test in testAssembly.Tests)
                    if (!ShouldSkip(test))
                        foreach (var parameters in FromInputAttributes(test))
                            await test.RunAsync(parameters);
            }
        }

        class CreateInstancePerCaseExplicitly : Execution
        {
            public async Task RunAsync(TestAssembly testAssembly)
            {
                foreach (var testClass in testAssembly.TestClasses)
                    foreach (var test in testClass.Tests)
                        if (!ShouldSkip(test))
                        {
                            var instance = testClass.Construct();
                            await test.RunAsync(instance);
                            await instance.DisposeIfApplicableAsync();
                        }
            }
        }

        class CreateInstancePerClass : Execution
        {
            public async Task RunAsync(TestAssembly testAssembly)
            {
                foreach (var testClass in testAssembly.TestClasses)
                {
                    var type = testClass.Type;
                    var instance = type.IsStatic() ? null : testClass.Construct();

                    foreach (var test in testClass.Tests)
                        if (!ShouldSkip(test))
                            foreach (var parameters in FromInputAttributes(test))
                                await test.RunAsync(instance, parameters);
                }
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
                ".ctor", "Fail",
                ".ctor",
                ".ctor", "Skip");
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
                ".ctor", "Fail",
                ".ctor", "Pass(1)",
                ".ctor", "Pass(2)");
        }

        public async Task ShouldFailCaseInAbsenseOfPrimaryCaseResultWhenConstructingPerCaseAndConstructorThrows()
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

        public async Task ShouldFailCaseInAbsenseOfPrimaryCaseResultWhenConstructingImplicitlyAndTestClassIsDisposable()
        {
            var output = await RunAsync<DisposableSampleTestClass, CreateInstancePerCase>();

            output.ShouldHaveResults(
                "DisposableSampleTestClass.Fail failed: Test class Fixie.Tests.TestClassConstructionTests+DisposableSampleTestClass is declared as disposable, which is firmly discouraged for test tear-down purposes. Test class disposal is not supported when the test runner is constructing test class instances implicitly. If you wish to use IDisposable or IDisposableAsync for test class tear down, perform construction and disposal explicitly in an implementation of Execution.RunAsync(...).",
                "DisposableSampleTestClass.Pass failed: Test class Fixie.Tests.TestClassConstructionTests+DisposableSampleTestClass is declared as disposable, which is firmly discouraged for test tear-down purposes. Test class disposal is not supported when the test runner is constructing test class instances implicitly. If you wish to use IDisposable or IDisposableAsync for test class tear down, perform construction and disposal explicitly in an implementation of Execution.RunAsync(...).",
                "DisposableSampleTestClass.Skip skipped: This test did not run.");

            output.ShouldHaveLifecycle();
        }

        public async Task ShouldFailCaseInAbsenseOfPrimaryCaseResultWhenConstructingImplicitlyAndTestClassIsAsyncDisposable()
        {
            var output = await RunAsync<AsyncDisposableSampleTestClass, CreateInstancePerCase>();

            output.ShouldHaveResults(
                "AsyncDisposableSampleTestClass.Fail failed: Test class Fixie.Tests.TestClassConstructionTests+AsyncDisposableSampleTestClass is declared as disposable, which is firmly discouraged for test tear-down purposes. Test class disposal is not supported when the test runner is constructing test class instances implicitly. If you wish to use IDisposable or IDisposableAsync for test class tear down, perform construction and disposal explicitly in an implementation of Execution.RunAsync(...).",
                "AsyncDisposableSampleTestClass.Pass failed: Test class Fixie.Tests.TestClassConstructionTests+AsyncDisposableSampleTestClass is declared as disposable, which is firmly discouraged for test tear-down purposes. Test class disposal is not supported when the test runner is constructing test class instances implicitly. If you wish to use IDisposable or IDisposableAsync for test class tear down, perform construction and disposal explicitly in an implementation of Execution.RunAsync(...).",
                "AsyncDisposableSampleTestClass.Skip skipped: This test did not run.");

            output.ShouldHaveLifecycle();
        }

        public async Task ShouldAllowExecutionWhenConstructingExplicitlyAndTestClassIsDisposable()
        {
            var output = await RunAsync<DisposableSampleTestClass, CreateInstancePerCaseExplicitly>();

            output.ShouldHaveResults(
                "DisposableSampleTestClass.Fail failed: 'Fail' failed!",
                "DisposableSampleTestClass.Pass passed",
                "DisposableSampleTestClass.Skip skipped: This test did not run.");

            output.ShouldHaveLifecycle(
                ".ctor", "Fail", "Dispose",
                ".ctor", "Pass", "Dispose");
        }

        public async Task ShouldAllowExecutionWhenConstructingExplicitlyAndTestClassIsAsyncDisposable()
        {
            var output = await RunAsync<AsyncDisposableSampleTestClass, CreateInstancePerCaseExplicitly>();

            output.ShouldHaveResults(
                "AsyncDisposableSampleTestClass.Fail failed: 'Fail' failed!",
                "AsyncDisposableSampleTestClass.Pass passed",
                "AsyncDisposableSampleTestClass.Skip skipped: This test did not run.");

            output.ShouldHaveLifecycle(
                ".ctor", "Fail", "DisposeAsync",
                ".ctor", "Pass", "DisposeAsync");
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
                "Pass(2)");
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

            output.ShouldHaveLifecycle(".ctor");
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