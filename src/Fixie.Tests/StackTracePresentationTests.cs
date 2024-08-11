using System.Diagnostics;
using Fixie.Reports;
using static Fixie.Tests.Utility;

namespace Fixie.Tests;

public class StackTracePresentationTests
{
    public async Task ShouldProvideCleanStackTraceForImplicitTestClassConstructionFailures()
    {
        (await Run<ConstructionFailureTestClass, ImplicitExceptionHandling>())
            .ShouldBe([
                $"Running Fixie.Tests (net{TargetFrameworkVersion})",
                "",
                "Test '" + FullName<ConstructionFailureTestClass>() + ".UnreachableTest' failed:",
                "",
                "'.ctor' failed!",
                "",
                "Fixie.Tests.FailureException",
                At<ConstructionFailureTestClass>(".ctor()"),
                "   at System.RuntimeType.CreateInstanceDefaultCtor(Boolean publicOnly, Boolean wrapExceptions)",
                "",
                "1 failed, took 1.23 seconds"
            ]);
    }
        
    public async Task ShouldNotAlterTheMeaningfulStackTraceOfExplicitTestClassConstructionFailures()
    {
        (await Run<ConstructionFailureTestClass, ExplicitExceptionHandling>())
            .ShouldBe([
                $"Running Fixie.Tests (net{TargetFrameworkVersion})",
                "",
                "Test '" + FullName<ConstructionFailureTestClass>() + ".UnreachableTest' failed:",
                "",
                "'.ctor' failed!",
                "",
                "Fixie.Tests.FailureException",
                At<ConstructionFailureTestClass>(".ctor()"),
                "   at System.RuntimeType.CreateInstanceDefaultCtor(Boolean publicOnly, Boolean wrapExceptions)",
                "--- End of stack trace from previous location ---",
                At(typeof(TestClass), "Construct(Object[] parameters)",
                    Path.Join("...", "src", "Fixie", "TestClass.cs")),
                At<ExplicitExceptionHandling>("Run(TestSuite testSuite)"),
                "",
                "1 failed, took 1.23 seconds"
            ]);
    }

    public async Task ShouldProvideCleanStackTraceTestMethodFailures()
    {
        (await Run<FailureTestClass, ImplicitExceptionHandling>())
            .ShouldBe([
                $"Running Fixie.Tests (net{TargetFrameworkVersion})",
                "",
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
                "2 failed, took 1.23 seconds"
            ]);
    }

    public async Task ShouldNotAlterTheMeaningfulStackTraceOfExplicitTestMethodInvocationFailures()
    {
        var output = (await Run<FailureTestClass, ExplicitExceptionHandling>()).ToArray();

        const string optimizedInvoker = "   at InvokeStub_FailureTestClass.Synchronous(Object, Object, IntPtr*)";
        const string initialInvoker = "   at System.RuntimeMethodHandle.InvokeMethod(Object target, Void** arguments, Signature sig, Boolean isConstructor)";

        output
            .ShouldBe([
                $"Running Fixie.Tests (net{TargetFrameworkVersion})",
                "",
                "Test '" + FullName<FailureTestClass>() + ".Asynchronous' failed:",
                "",
                "'Asynchronous' failed!",
                "",
                "Fixie.Tests.FailureException",
                At<FailureTestClass>("Asynchronous()"),
                At(typeof(MethodInfoExtensions),
                    "CallResolvedMethod(MethodInfo resolvedMethod, Object instance, Object[] parameters)",
                    Path.Join("...", "src", "Fixie", "MethodInfoExtensions.cs")),
                At(typeof(MethodInfoExtensions), "Call(MethodInfo method, Object instance, Object[] parameters)",
                    Path.Join("...", "src", "Fixie", "MethodInfoExtensions.cs")),
                At<ExplicitExceptionHandling>("Run(TestSuite testSuite)"),
                "",
                "Test '" + FullName<FailureTestClass>() + ".Synchronous' failed:",
                "",
                "'Synchronous' failed!",
                "",
                "Fixie.Tests.FailureException",
                At<FailureTestClass>("Synchronous()"),
                output.Contains(optimizedInvoker)
                    ? optimizedInvoker
                    : initialInvoker,
                "   at System.Reflection.MethodBaseInvoker.InvokeWithNoArgs(Object obj, BindingFlags invokeAttr)",
                "--- End of stack trace from previous location ---",
                At(typeof(MethodInfoExtensions),
                    "CallResolvedMethod(MethodInfo resolvedMethod, Object instance, Object[] parameters)",
                    Path.Join("...", "src", "Fixie", "MethodInfoExtensions.cs")),
                At(typeof(MethodInfoExtensions), "Call(MethodInfo method, Object instance, Object[] parameters)",
                    Path.Join("...", "src", "Fixie", "MethodInfoExtensions.cs")),
                At<ExplicitExceptionHandling>("Run(TestSuite testSuite)"),
                "",
                "2 failed, took 1.23 seconds"
            ]);
    }

    public async Task ShouldProvideLiterateStackTraceIncludingAllNestedExceptions()
    {
        (await Run<NestedFailureTestClass, ImplicitExceptionHandling>())
            .ShouldBe([
                $"Running Fixie.Tests (net{TargetFrameworkVersion})",
                "",
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
                "2 failed, took 1.23 seconds"
            ]);
    }

    static async Task<IEnumerable<string>> Run<TSampleTestClass, TExecution>() where TExecution : IExecution, new()
    {
        var discovery = new SelfTestDiscovery();
        var execution = new TExecution();

        await using var console = new StringWriter();

        var report = new ConsoleReport(GetTestEnvironment(console));
        
        await Utility.Run(report, discovery, execution, console, typeof(TSampleTestClass));

        return console.ToString()
            .NormalizeStackTraces()
            .Lines()
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
                    var startTime = Stopwatch.GetTimestamp();

                    try
                    {
                        var instance = testClass.Construct();

                        await test.Method.Call(instance);

                        await test.Pass(Stopwatch.GetElapsedTime(startTime));
                    }
                    catch (Exception exception)
                    {
                        await test.Fail(exception, Stopwatch.GetElapsedTime(startTime));
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