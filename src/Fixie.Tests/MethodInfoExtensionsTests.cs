using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.FSharp.Control;
using static System.Environment;
using static Fixie.Tests.Utility;

namespace Fixie.Tests;

public class MethodInfoExtensionsTests : InstrumentedExecutionTests
{
    class MethodInfoAccessingExecution : IExecution
    {
        public async Task Run(TestSuite testSuite)
        {
            foreach (var testClass in testSuite.TestClasses)
                foreach (var test in testClass.Tests)
                    foreach (var parameters in FromInputAttributes(test))
                    {
                        var startTime = Stopwatch.GetTimestamp();

                        try
                        {
                            var instance = testClass.Construct();

                            var result = await test.Method.Call(instance, parameters);

                            if (result != null)
                                ConsoleWriteLine($"{test.Method.Name} resulted in {result.GetType().FullName} with value {result}");
                            else if (test.Method.ReturnType != typeof(void) &&
                                     test.Method.ReturnType != typeof(Task) &&
                                     test.Method.ReturnType != typeof(ValueTask))
                                ConsoleWriteLine($"{test.Method.Name} resulted in null");

                            await test.Pass(parameters, Stopwatch.GetElapsedTime(startTime));
                        }
                        catch (Exception exception)
                        {
                            await test.Fail(parameters, exception, Stopwatch.GetElapsedTime(startTime));
                        }
                    }
        }
    }

    public async Task ShouldCallSynchronousMethods()
    {
        var output = await Run<SyncTestClass, MethodInfoAccessingExecution>();

        output.ShouldHaveResults(
            "SyncTestClass.Args(1, 2, 3) passed",
            "SyncTestClass.Args(1, 2, 3, \"Extra\") failed: Parameter count mismatch.",
            "SyncTestClass.ReturnsInteger passed",
            "SyncTestClass.ReturnsNull passed",
            "SyncTestClass.ReturnsObject passed",
            "SyncTestClass.ReturnsString passed",
            "SyncTestClass.Throws failed: 'Throws' failed!",
            "SyncTestClass.ZeroArgs passed");

        output.ShouldHaveLifecycle(
            "Args",
            "ReturnsInteger",
            "ReturnsInteger resulted in System.Int32 with value 42",
            "ReturnsNull",
            "ReturnsNull resulted in null",
            "ReturnsObject",
            "ReturnsObject resulted in Fixie.Tests.MethodInfoExtensionsTests+Example with value <example>",
            "ReturnsString",
            "ReturnsString resulted in System.String with value ABC",
            "Throws",
            "ZeroArgs");
    }

    public async Task ShouldResolveGenericTypeParametersWhenPossible()
    {
        var output = await Run<GenericTestClass, MethodInfoAccessingExecution>();

        output.ShouldHaveResults(
            "GenericTestClass.Args<System.Int32, System.Int32>(1, 2, System.Int32, System.Int32) passed",
            "GenericTestClass.Args<System.Char, System.Double>('a', 3, System.Char, System.Double) passed",

            "GenericTestClass.ConstrainedArgs<System.Int32, System.Char>(1, 'a', System.Int32, System.Char) passed",
            "GenericTestClass.ConstrainedArgs<System.Int32, System.Char>(2, 'b', System.Int32, System.Int32) failed: typeof(T2) should be System.Int32 but was System.Char",
            "GenericTestClass.ConstrainedArgs<T1, T2>(1, null, System.Int32, System.Object) failed: The type parameters for generic method ConstrainedArgs could not be resolved.",
            "GenericTestClass.ConstrainedArgs<T1, T2>(null, 2, System.Object, System.Int32) failed: The type parameters for generic method ConstrainedArgs could not be resolved.",

            "GenericTestClass.NullableValueTypeArgs<System.Int32, System.Int32>(1, 2, System.Int32, System.Int32) passed",
            "GenericTestClass.NullableValueTypeArgs<System.Char, System.Double>('a', 3, System.Char, System.Double) passed",
            "GenericTestClass.NullableValueTypeArgs<T1, T2>(1, null, System.Int32, System.Object) failed: The type parameters for generic method NullableValueTypeArgs could not be resolved.");

        output.ShouldHaveLifecycle("Args", "Args", "ConstrainedArgs", "ConstrainedArgs", "NullableValueTypeArgs", "NullableValueTypeArgs");
    }

    public async Task ShouldAwaitAsynchronousMethodsToEnsureCompleteExecution()
    {
        var output = await Run<AsyncTestClass, MethodInfoAccessingExecution>();

        output.ShouldHaveResults(
            "AsyncTestClass.AwaitTaskThenPass passed",
            "AsyncTestClass.AwaitValueTaskThenPass passed",
            "AsyncTestClass.CompleteTaskThenPass passed",
            "AsyncTestClass.FailAfterAwaitTask failed: result should be 0 but was 3",
            "AsyncTestClass.FailAfterAwaitValueTask failed: result should be 0 but was 3",
            "AsyncTestClass.FailBeforeAwaitTask failed: 'FailBeforeAwaitTask' failed!",
            "AsyncTestClass.FailBeforeAwaitValueTask failed: 'FailBeforeAwaitValueTask' failed!",
            "AsyncTestClass.FailDuringAwaitTask failed: Attempted to divide by zero.",
            "AsyncTestClass.FailDuringAwaitValueTask failed: Attempted to divide by zero.",
            "AsyncTestClass.GenericAsyncTaskFail failed: Attempted to divide by zero.",
            "AsyncTestClass.GenericAsyncTaskWithResult passed",
            "AsyncTestClass.GenericAsyncValueTaskFail failed: Attempted to divide by zero.",
            "AsyncTestClass.GenericAsyncValueTaskWithResult passed",
            "AsyncTestClass.GenericTaskFail failed: One or more errors occurred. (Attempted to divide by zero.)",
            "AsyncTestClass.GenericTaskWithResult passed",
            "AsyncTestClass.NullTask failed: The asynchronous method NullTask returned null, " +
            "but a non-null awaitable object was expected.");

        output.ShouldHaveLifecycle(
            "AwaitTaskThenPass",
            "AwaitValueTaskThenPass",
            "CompleteTaskThenPass",
            "FailAfterAwaitTask",
            "FailAfterAwaitValueTask",
            "FailBeforeAwaitTask",
            "FailBeforeAwaitValueTask",
            "FailDuringAwaitTask",
            "FailDuringAwaitValueTask",
            "GenericAsyncTaskFail",
            "GenericAsyncTaskWithResult",
            "GenericAsyncTaskWithResult resulted in System.Int32 with value 3",
            "GenericAsyncValueTaskFail",
            "GenericAsyncValueTaskWithResult",
            "GenericAsyncValueTaskWithResult resulted in System.Int32 with value 4",
            "GenericTaskFail",
            "GenericTaskWithResult",
            "GenericTaskWithResult resulted in System.Boolean with value True",
            "NullTask");
    }

    public async Task ShouldRunFSharpAsyncResultsToEnsureCompleteExecution()
    {
        var output = await Run<FSharpAsyncTestClass, MethodInfoAccessingExecution>();

        output.ShouldHaveResults(
            "FSharpAsyncTestClass.AsyncPass passed",
            "FSharpAsyncTestClass.FailBeforeAsync failed: 'FailBeforeAsync' failed!",
            "FSharpAsyncTestClass.FailFromAsync failed: result should be 0 but was 3",
            "FSharpAsyncTestClass.NullAsync failed: The asynchronous method NullAsync returned null, " +
            "but a non-null awaitable object was expected.");

        output.ShouldHaveLifecycle(
            "AsyncPass",
            "AsyncPass resulted in System.Int32 with value 3",
            "FailBeforeAsync",
            "FailFromAsync",
            "NullAsync");
    }

    public async Task ShouldThrowWithClearExplanationWhenMethodReturnsNonStartedTask()
    {
        var output = await Run<FailDueToNonStartedTaskTestClass, MethodInfoAccessingExecution>();

        output.ShouldHaveResults(
            "FailDueToNonStartedTaskTestClass.Test failed: The method Test returned a non-started task, which cannot " +
            "be awaited. Consider using Task.Run or Task.Factory.StartNew.");

        output.ShouldHaveLifecycle("Test");
    }

    public async Task ShouldThrowForUnsupportedReturnTypeDeclarationsRatherThanAttemptExecution()
    {
        var output = await Run<UnsupportedReturnTypeDeclarationsTestClass, MethodInfoAccessingExecution>();

        output.ShouldHaveResults(
            "UnsupportedReturnTypeDeclarationsTestClass.AsyncEnumerable failed: " +
            "The return type of method AsyncEnumerable is an unsupported awaitable type. " +
            "To ensure the reliability of the test runner, declare " +
            "the method return type as `Task`, `Task<T>`, `ValueTask`, " +
            "or `ValueTask<T>`.",

            "UnsupportedReturnTypeDeclarationsTestClass.AsyncEnumerator failed: " +
            "The return type of method AsyncEnumerator is an unsupported awaitable type. " +
            "To ensure the reliability of the test runner, declare " +
            "the method return type as `Task`, `Task<T>`, `ValueTask`, " +
            "or `ValueTask<T>`.",

            "UnsupportedReturnTypeDeclarationsTestClass.AsyncVoid failed: " +
            "The method AsyncVoid is declared as `async void`, which is not supported. " +
            "To ensure the reliability of the test runner, declare " +
            "the method as `async Task`.",

            "UnsupportedReturnTypeDeclarationsTestClass.UntrustworthyAwaitable failed: " +
            "The return type of method UntrustworthyAwaitable is an unsupported awaitable type. " +
            "To ensure the reliability of the test runner, declare " +
            "the method return type as `Task`, `Task<T>`, `ValueTask`, " +
            "or `ValueTask<T>`."
        );

        output.ShouldHaveLifecycle();
    }

    static void ThrowException([CallerMemberName] string member = default!)
    {
        throw new FailureException(member);
    }

    static Task<int> DivideAsync(int numerator, int denominator)
    {
        return Task.Run(() => numerator/denominator);
    }

    static int Divide(int numerator, int denominator)
    {
        return numerator/denominator;
    }

    class SyncTestClass
    {
        public void ZeroArgs() { WhereAmI(); }

        public void Throws() { WhereAmI(); ThrowException(); }

        [Input(1, 2, 3)]
        [Input(1, 2, 3, "Extra")]
        public void Args(int a, int b, int c) { WhereAmI(); }

        public int ReturnsInteger()
        {
            WhereAmI();

            return 42;
        }

        public object? ReturnsNull()
        {
            WhereAmI();

            return null;
        }

        public object ReturnsObject()
        {
            WhereAmI();

            return new Example("example");
        }

        public string ReturnsString()
        {
            WhereAmI();

            return "ABC";
        }
    }

    public class Example
    {
        readonly string name;

        public Example(string name) => this.name = name;
            
        public override string ToString() => $"<{name}>";
    }

    class GenericTestClass
    {
        [Input(1, 2, typeof(int), typeof(int))]
        [Input('a', 3.0d, typeof(char), typeof(double))]
        public void Args<T1, T2>(T1 a, T2 b, Type expectedT1, Type expectedT2)
        {
            WhereAmI();
            typeof(T1).ShouldBe(expectedT1);
            typeof(T2).ShouldBe(expectedT2);
        }

        [Input(1, 'a', typeof(int), typeof(char))]
        [Input(2, 'b', typeof(int), typeof(int))]
        [Input(1, null, typeof(int), typeof(object))]
        [Input(null, 2, typeof(object), typeof(int))]
        public void ConstrainedArgs<T1, T2>(T1 a, T2 b, Type expectedT1, Type expectedT2)
            where T1: struct
            where T2: struct
        {
            WhereAmI();
            typeof(T1).ShouldBe(expectedT1);
            typeof(T2).ShouldBe(expectedT2);
        }

        [Input(1, 2, typeof(int), typeof(int))]
        [Input('a', 3.0d, typeof(char), typeof(double))]
        [Input(1, null, typeof(int), typeof(object))]
        public void NullableValueTypeArgs<T1, T2>(T1 a, T2? b, Type expectedT1, Type expectedT2) where T2: struct
        {
            WhereAmI();
            typeof(T1).ShouldBe(expectedT1);
            typeof(T2).ShouldBe(expectedT2);
        }
    }

    class AsyncTestClass
    {
        public async Task AwaitTaskThenPass()
        {
            WhereAmI();

            var result = await DivideAsync(15, 5);

            result.ShouldBe(3);
        }

        public async ValueTask AwaitValueTaskThenPass()
        {
            WhereAmI();

            var result = await DivideAsync(15, 5);

            result.ShouldBe(3);
        }

        public Task CompleteTaskThenPass()
        {
            WhereAmI();

            var divide = DivideAsync(15, 5);

            return divide.ContinueWith(division =>
            {
                division.Result.ShouldBe(3);
            });
        }

        public async Task FailAfterAwaitTask()
        {
            WhereAmI();

            var result = await DivideAsync(15, 5);

            result.ShouldBe(0);
        }

        public async ValueTask FailAfterAwaitValueTask()
        {
            WhereAmI();

            var result = await DivideAsync(15, 5);

            result.ShouldBe(0);
        }

        public async Task FailBeforeAwaitTask()
        {
            WhereAmI();

            ThrowException();

            await DivideAsync(15, 5);
        }

        public async ValueTask FailBeforeAwaitValueTask()
        {
            WhereAmI();

            ThrowException();

            await DivideAsync(15, 5);
        }

        public async Task FailDuringAwaitTask()
        {
            WhereAmI();

            await DivideAsync(15, 0);

            throw new ShouldBeUnreachableException();
        }

        public async ValueTask FailDuringAwaitValueTask()
        {
            WhereAmI();

            await DivideAsync(15, 0);

            throw new ShouldBeUnreachableException();
        }

        public async Task<int> GenericAsyncTaskFail()
        {
            WhereAmI();

            return await DivideAsync(15, 0);
        }

        public async Task<int> GenericAsyncTaskWithResult()
        {
            WhereAmI();

            return await DivideAsync(15, 5);
        }

        public async ValueTask<int> GenericAsyncValueTaskWithResult()
        {
            WhereAmI();

            return await DivideAsync(20, 5);
        }

        public async ValueTask<int> GenericAsyncValueTaskFail()
        {
            WhereAmI();

            return await DivideAsync(15, 0);
        }

        public Task<bool> GenericTaskFail()
        {
            WhereAmI();

            var divide = DivideAsync(15, 0);

            return divide.ContinueWith(division => division.Result == 42);
        }

        public Task<bool> GenericTaskWithResult()
        {
            WhereAmI();

            var divide = DivideAsync(15, 5);

            return divide.ContinueWith(division => division.Result == 3);
        }

        public Task? NullTask()
        {
            WhereAmI();

            // Although unlikely, we must ensure that
            // we don't attempt to wait on a Task that
            // is in fact null.
            return null;
        }
    }

    class FSharpAsyncTestClass
    {
        public FSharpAsync<int> AsyncPass()
        {
            WhereAmI();

            return new FSharpAsync<int>(() =>
            {
                var result = Divide(15, 5);

                result.ShouldBe(3);

                return result;
            });
        }

        public FSharpAsync<int> FailBeforeAsync()
        {
            WhereAmI();

            ThrowException();

            return new FSharpAsync<int>(() => Divide(15, 5));
        }

        public FSharpAsync<int> FailFromAsync()
        {
            WhereAmI();

            return new FSharpAsync<int>(() =>
            {
                var result = Divide(15, 5);

                result.ShouldBe(0);

                return result;
            });
        }

        public FSharpAsync<int>? NullAsync()
        {
            WhereAmI();

            // Although unlikely, we must ensure that
            // we don't attempt to wait on an FSharpAsync that
            // is in fact null.
            return null;
        }
    }

    class FailDueToNonStartedTaskTestClass
    {
        public Task Test()
        {
            WhereAmI();

            return new Task(() => throw new ShouldBeUnreachableException());
        }
    }

    class UnsupportedReturnTypeDeclarationsTestClass
    {
        public async void AsyncVoid()
        {
            WhereAmI();

            await DivideAsync(15, 5);
            throw new ShouldBeUnreachableException();
        }

        public async UntrustworthyAwaitable UntrustworthyAwaitable()
        {
            WhereAmI();

            await DivideAsync(15, 0);

            throw new ShouldBeUnreachableException();
        }

        public async IAsyncEnumerable<int> AsyncEnumerable()
        {
            WhereAmI();

            yield return await DivideAsync(15, 5);

            throw new ShouldBeUnreachableException();
        }

        public async IAsyncEnumerator<int> AsyncEnumerator()
        {
            WhereAmI();

            yield return await DivideAsync(15, 5);

            throw new ShouldBeUnreachableException();
        }
    }
}