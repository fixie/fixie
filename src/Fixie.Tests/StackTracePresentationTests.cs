namespace Fixie.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Assertions;
    using Fixie.Reports;
    using static Utility;

    public class StackTracePresentationTests
    {
        public async Task ShouldProvideCleanStackTraceForImplicitTestClassConstructionFailures()
        {
            (await RunAsync<ConstructionFailureTestClass, ImplicitConstruction>())
                .ShouldBe(
                    "Test '" + FullName<ConstructionFailureTestClass>() + ".UnreachableTest' failed:",
                    "",
                    "'.ctor' failed!",
                    "",
                    "Fixie.Tests.FailureException",
                    At<ConstructionFailureTestClass>(".ctor()"),
                    "",
                    "1 failed, took 1.23 seconds");
        }
        
        public async Task ShouldProvideCleanStackTraceForExplicitTestClassConstructionFailures()
        {
            (await RunAsync<ConstructionFailureTestClass, ExplicitConstruction>())
                .ShouldBe(
                    "Test '" + FullName<ConstructionFailureTestClass>() + ".UnreachableTest' failed:",
                    "",
                    "'.ctor' failed!",
                    "",
                    "Fixie.Tests.FailureException",
                    At<ConstructionFailureTestClass>(".ctor()"),
                    "",
                    "Test '" + FullName<ConstructionFailureTestClass>() + ".UnreachableTest' skipped:",
                    "This test did not run.",
                    "",
                    "1 failed, 1 skipped, took 1.23 seconds");
        }

        public async Task ShouldProvideCleanStackTraceTestMethodFailures()
        {
            (await RunAsync<FailureTestClass, ImplicitConstruction>())
                .ShouldBe(
                    "Test '" + FullName<FailureTestClass>() + ".Asynchronous' failed:",
                    "",
                    "'Asynchronous' failed!",
                    "",
                    "Fixie.Tests.FailureException",
                    At<FailureTestClass>("Asynchronous()"),
                    "",
                    "Test '" + FullName<FailureTestClass>() + ".Synchronous' failed:",
                    "",
                    "'Synchronous' failed!",
                    "",
                    "Fixie.Tests.FailureException",
                    At<FailureTestClass>("Synchronous()"),
                    "",
                    "2 failed, took 1.23 seconds");
        }

        public async Task ShouldProvideLiterateStackTraceIncludingAllNestedExceptions()
        {
            (await RunAsync<NestedFailureTestClass, ImplicitConstruction>())
                .ShouldBe(
                    "Test '" + FullName<NestedFailureTestClass>() + ".Asynchronous' failed:",
                    "",
                    "Primary Exception!",
                    "",
                    FullName<PrimaryException>(),
                    At<StackTracePresentationTests>("ThrowNestedException()"),
                    At<NestedFailureTestClass>("Asynchronous()"),
                    "",
                    "------- Inner Exception: System.AggregateException -------",
                    "One or more errors occurred. (Divide by Zero Exception!)",
                    At<StackTracePresentationTests>("ThrowNestedException()"),
                    "",
                    "------- Inner Exception: System.DivideByZeroException -------",
                    "Divide by Zero Exception!",
                    At<StackTracePresentationTests>("ThrowNestedException()"),
                    "",
                    "Test '" + FullName<NestedFailureTestClass>() + ".Synchronous' failed:",
                    "",
                    "Primary Exception!",
                    "",
                    FullName<PrimaryException>(),
                    At<StackTracePresentationTests>("ThrowNestedException()"),
                    At<NestedFailureTestClass>("Synchronous()"),
                    "",
                    "------- Inner Exception: System.AggregateException -------",
                    "One or more errors occurred. (Divide by Zero Exception!)",
                    At<StackTracePresentationTests>("ThrowNestedException()"),
                    "",
                    "------- Inner Exception: System.DivideByZeroException -------",
                    "Divide by Zero Exception!",
                    At<StackTracePresentationTests>("ThrowNestedException()"),
                    "",
                    "2 failed, took 1.23 seconds");
        }

        static async Task<IEnumerable<string>> RunAsync<TSampleTestClass, TExecution>() where TExecution : Execution, new()
        {
            var discovery = new SelfTestDiscovery();
            var execution = new TExecution();
            
            using var console = new RedirectedConsole();

            var report = new ConsoleReport(System.Console.Out);
            
            await Utility.RunAsync(report, discovery, execution, typeof(TSampleTestClass));

            return console.Lines()
                .CleanStackTraceLineNumbers()
                .CleanDuration();
        }

        class ImplicitConstruction : Execution
        {
            public async Task RunAsync(TestAssembly testAssembly)
            {
                foreach (var test in testAssembly.Tests)
                    await test.RunAsync();
            }
        }

        class ExplicitConstruction : Execution
        {
            public async Task RunAsync(TestAssembly testAssembly)
            {
                foreach (var testClass in testAssembly.TestClasses)
                {
                    var instance = testClass.Construct();
                    foreach (var test in testClass.Tests)
                        await test.RunAsync(instance);
                }
            }
        }

        class ConstructionFailureTestClass
        {
            public ConstructionFailureTestClass() => throw new FailureException();
            public void UnreachableTest() => throw new ShouldBeUnreachableException();
        }

        class FailureTestClass
        {
            public void Synchronous()
            {
                throw new FailureException();
            }

            public async Task Asynchronous()
            {
                await Task.Yield();
                throw new FailureException();
            }
        }

        class NestedFailureTestClass
        {
            public void Synchronous()
            {
                ThrowNestedException();
            }

            public async Task Asynchronous()
            {
                await Task.Yield();
                ThrowNestedException();
            }
        }

        static void ThrowNestedException()
        {
            try
            {
                try
                {
                    throw new DivideByZeroException("Divide by Zero Exception!");
                }
                catch (Exception exception)
                {
                    throw new AggregateException(exception);
                }
            }
            catch (Exception exception)
            {
                throw new PrimaryException(exception);
            }
        }

        class PrimaryException : Exception
        {
            public PrimaryException(Exception innerException)
                : base("Primary Exception!", innerException) { }
        }
    }
}