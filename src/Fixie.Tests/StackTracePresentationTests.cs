namespace Fixie.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Assertions;
    using Fixie.Internal;
    using Fixie.Internal.Listeners;
    using static Utility;

    public class StackTracePresentationTests
    {
        public void ShouldProvideCleanStackTraceForImplicitTestClassConstructionFailures()
        {
            Run<ConstructionFailureTestClass, ImplicitConstruction>()
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
        
        public void ShouldProvideCleanStackTraceForExplicitTestClassConstructionFailures()
        {
            Run<ConstructionFailureTestClass, ExplicitConstruction>()
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

        public void ShouldProvideCleanStackTraceTestMethodFailures()
        {
            Run<FailureTestClass, ImplicitConstruction>()
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

        public void ShouldProvideLiterateStackTraceIncludingAllNestedExceptions()
        {
            Run<NestedFailureTestClass, ImplicitConstruction>()
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

        static IEnumerable<string> Run<TSampleTestClass, TExecution>() where TExecution : Execution, new()
        {
            var listener = new ConsoleListener();
            var discovery = new SelfTestDiscovery();
            var execution = new TExecution();
            
            using var console = new RedirectedConsole();

            Utility.Run(listener, discovery, execution, typeof(TSampleTestClass)).GetAwaiter().GetResult();

            return console.Lines()
                .CleanStackTraceLineNumbers()
                .CleanDuration();
        }

        class ImplicitConstruction : Execution
        {
            public async Task Execute(TestClass testClass)
            {
                foreach (var test in testClass.Tests)
                    await test.Run();
            }
        }

        class ExplicitConstruction : Execution
        {
            public async Task Execute(TestClass testClass)
            {
                var instance = testClass.Construct();
                foreach (var test in testClass.Tests)
                    await test.Run(instance);
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