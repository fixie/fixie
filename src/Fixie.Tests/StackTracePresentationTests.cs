namespace Fixie.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Assertions;
    using Fixie.Reports;
    using static Utility;

    public class StackTracePresentationTests
    {
        public async Task ShouldProvideCleanStackTraceForImplicitTestClassConstructionFailures()
        {
            (await Run<ConstructionFailureTestClass, ImplicitExceptionHandling>())
                .ShouldBe(
                    "Test '" + FullName<ConstructionFailureTestClass>() + ".UnreachableTest' failed:",
                    "",
                    "'.ctor' failed!",
                    "",
                    "Fixie.Tests.FailureException",
                    At<ConstructionFailureTestClass>(".ctor()"),
                    #if !NETCOREAPP3_1
                    "   at System.RuntimeType.CreateInstanceDefaultCtor(Boolean publicOnly, Boolean wrapExceptions)",
                    #endif
                    "",
                    "1 failed, took 1.23 seconds");
        }
        
        public async Task ShouldNotAlterTheMeaningfulStackTraceOfExplicitTestClassConstructionFailures()
        {
            (await Run<ConstructionFailureTestClass, ExplicitExceptionHandling>())
                .ShouldBe(
                    "Test '" + FullName<ConstructionFailureTestClass>() + ".UnreachableTest' failed:",
                    "",
                    "'.ctor' failed!",
                    "",
                    "Fixie.Tests.FailureException",
                    At<ConstructionFailureTestClass>(".ctor()"),
                    #if !NETCOREAPP3_1
                    "   at System.RuntimeType.CreateInstanceDefaultCtor(Boolean publicOnly, Boolean wrapExceptions)",
                    #endif
                    "--- End of stack trace from previous location where exception was thrown ---",
                    At(typeof(TestClass), "Construct(Object[] parameters)", Path.Join("...", "src", "Fixie", "TestClass.cs")),
                    At<ExplicitExceptionHandling>("Run(TestSuite testSuite)"),
                    "",
                    "1 failed, took 1.23 seconds");
        }

        public async Task ShouldProvideCleanStackTraceTestMethodFailures()
        {
            (await Run<FailureTestClass, ImplicitExceptionHandling>())
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
            var output = (await Run<FailureTestClass, ExplicitExceptionHandling>()).ToArray();

            #if NET7_0_OR_GREATER
            const string optimizedInvoker = "   at InvokeStub_FailureTestClass.Synchronous(Object, Object, IntPtr*)";
            const string initialInvoker = "   at System.RuntimeMethodHandle.InvokeMethod(Object target, Void** arguments, Signature sig, Boolean isConstructor)";
            #endif

            output
                .ShouldBe(
                    "Test '" + FullName<FailureTestClass>() + ".Asynchronous' failed:",
                    "",
                    "'Asynchronous' failed!",
                    "",
                    "Fixie.Tests.FailureException",
                    At<FailureTestClass>("Asynchronous()"),
                    At(typeof(MethodInfoExtensions), "CallResolvedMethod(MethodInfo resolvedMethod, Object instance, Object[] parameters)", Path.Join("...", "src", "Fixie", "MethodInfoExtensions.cs")),
                    At(typeof(MethodInfoExtensions), "Call(MethodInfo method, Object instance, Object[] parameters)", Path.Join("...", "src", "Fixie", "MethodInfoExtensions.cs")),
                    At<ExplicitExceptionHandling>("Run(TestSuite testSuite)"),
                    "",
                    "Test '" + FullName<FailureTestClass>() + ".Synchronous' failed:",
                    "",
                    "'Synchronous' failed!",
                    "",
                    "Fixie.Tests.FailureException",
                    At<FailureTestClass>("Synchronous()"),
                    #if NET7_0
                    output.Contains(optimizedInvoker)
                        ? optimizedInvoker
                        : initialInvoker,
                    "   at System.Reflection.MethodInvoker.Invoke(Object obj, IntPtr* args, BindingFlags invokeAttr)",
                    #endif
                    #if NET8_0_OR_GREATER
                    output.Contains(optimizedInvoker)
                        ? optimizedInvoker
                        : initialInvoker,
                    "   at System.Reflection.MethodBaseInvoker.InvokeWithNoArgs(Object obj, BindingFlags invokeAttr)",
                    #endif
                    "--- End of stack trace from previous location where exception was thrown ---",
                    At(typeof(MethodInfoExtensions), "CallResolvedMethod(MethodInfo resolvedMethod, Object instance, Object[] parameters)", Path.Join("...", "src", "Fixie", "MethodInfoExtensions.cs")),
                    At(typeof(MethodInfoExtensions), "Call(MethodInfo method, Object instance, Object[] parameters)", Path.Join("...", "src", "Fixie", "MethodInfoExtensions.cs")),
                    At<ExplicitExceptionHandling>("Run(TestSuite testSuite)"),
                    "",
                    "2 failed, took 1.23 seconds");
        }

        public async Task ShouldProvideLiterateStackTraceIncludingAllNestedExceptions()
        {
            (await Run<NestedFailureTestClass, ImplicitExceptionHandling>())
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

        static async Task<IEnumerable<string>> Run<TSampleTestClass, TExecution>() where TExecution : IExecution, new()
        {
            var discovery = new SelfTestDiscovery();
            var execution = new TExecution();

            using var console = new RedirectedConsole();

            var report = new ConsoleReport(GetTestEnvironment());
            
            await Utility.Run(report, discovery, execution, typeof(TSampleTestClass));

            return console.Lines()
                .NormalizeStackTraceLines()
                .CleanDuration();
        }

        class ImplicitExceptionHandling : IExecution
        {
            public async Task Run(TestSuite testSuite)
            {
                foreach (var test in testSuite.Tests)
                    await test.Run();
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

                            await test.Method.Call(instance);

                            await test.Pass();
                        }
                        catch (Exception exception)
                        {
                            await test.Fail(exception);
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