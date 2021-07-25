namespace Fixie.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using Assertions;
    using Fixie.Internal;
    using Fixie.Reports;
    using static Utility;

    public class StackTracePresentationTests
    {
        public async Task ShouldProvideCleanStackTraceForImplicitTestClassConstructionFailures()
        {
            (await RunAsync<ConstructionFailureTestClass, ImplicitExceptionHandling>())
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
        
        public async Task ShouldNotAlterTheMeaningfulStackTraceOfExplicitTestClassConstructionFailures()
        {
            (await RunAsync<ConstructionFailureTestClass, ExplicitExceptionHandling>())
                .ShouldBe(
                    "Test '" + FullName<ConstructionFailureTestClass>() + ".UnreachableTest' failed:",
                    "",
                    "'.ctor' failed!",
                    "",
                    "Fixie.Tests.FailureException",
                    At<ConstructionFailureTestClass>(".ctor()"),
                    "--- End of stack trace from previous location where exception was thrown ---",
                    At(typeof(TestClass), "Construct(Object[] parameters)", Path.Join("...", "src", "Fixie", "TestClass.cs")),
                    At<ExplicitExceptionHandling>("Run(TestSuite testSuite)"),
                    "",
                    "1 failed, took 1.23 seconds");
        }

        public async Task ShouldProvideCleanStackTraceTestMethodFailures()
        {
            (await RunAsync<FailureTestClass, ImplicitExceptionHandling>())
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

        public async Task ShouldNotAlterTheMeaningfulStackTraceOfExplicitTestMethodInvocationFailures()
        {
            (await RunAsync<FailureTestClass, ExplicitExceptionHandling>())
                .ShouldBe(
                    "Test '" + FullName<FailureTestClass>() + ".Asynchronous' failed:",
                    "",
                    "'Asynchronous' failed!",
                    "",
                    "Fixie.Tests.FailureException",
                    At<FailureTestClass>("Asynchronous()"),
                    At(typeof(MethodInfoExtensions), "CallResolvedMethodAsync(MethodInfo resolvedMethod, Object instance, Object[] parameters)", Path.Join("...", "src", "Fixie", "MethodInfoExtensions.cs")),
                    At(typeof(MethodInfoExtensions), "CallAsync(MethodInfo method, Object instance, Object[] parameters)", Path.Join("...", "src", "Fixie", "MethodInfoExtensions.cs")),
                    At<ExplicitExceptionHandling>("Run(TestSuite testSuite)"),
                    "",
                    "Test '" + FullName<FailureTestClass>() + ".Synchronous' failed:",
                    "",
                    "'Synchronous' failed!",
                    "",
                    "Fixie.Tests.FailureException",
                    At<FailureTestClass>("Synchronous()"),
                    "--- End of stack trace from previous location where exception was thrown ---",
                    At(typeof(MethodInfoExtensions), "CallResolvedMethodAsync(MethodInfo resolvedMethod, Object instance, Object[] parameters)", Path.Join("...", "src", "Fixie", "MethodInfoExtensions.cs")),
                    At(typeof(MethodInfoExtensions), "CallAsync(MethodInfo method, Object instance, Object[] parameters)", Path.Join("...", "src", "Fixie", "MethodInfoExtensions.cs")),
                    At<ExplicitExceptionHandling>("Run(TestSuite testSuite)"),
                    "",
                    "2 failed, took 1.23 seconds");
        }

        public async Task ShouldProvideLiterateStackTraceIncludingAllNestedExceptions()
        {
            (await RunAsync<NestedFailureTestClass, ImplicitExceptionHandling>())
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

        static async Task<IEnumerable<string>> RunAsync<TSampleTestClass, TExecution>() where TExecution : IExecution, new()
        {
            var convention = new Convention(new SelfTestDiscovery(), new TExecution());
            
            using var console = new RedirectedConsole();

            var report = new ConsoleReport(System.Console.Out);
            
            await Utility.RunAsync(report, convention, typeof(TSampleTestClass));

            return console.Lines()
                .NormalizeStackTraceLines()
                .CleanDuration();
        }

        class ImplicitExceptionHandling : IExecution
        {
            public async Task Run(TestSuite testSuite)
            {
                foreach (var test in testSuite.Tests)
                    await test.RunAsync();
            }
        }

        class ExplicitExceptionHandling : IExecution
        {
            public async Task Run(TestSuite testSuite)
            {
                foreach (var testClass in testSuite.TestClasses)
                {
                    foreach (var test in testClass.Tests)
                    {
                        try
                        {
                            var instance = testClass.Construct();

                            await test.Method.CallAsync(instance);

                            await test.PassAsync();
                        }
                        catch (Exception exception)
                        {
                            await test.FailAsync(exception);
                        }
                    }
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