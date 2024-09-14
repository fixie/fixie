using System.Runtime.CompilerServices;
using Microsoft.FSharp.Control;

namespace Fixie.Tests;

public class ReturnTypeTests : InstrumentedExecutionTests
{
    public async Task ShouldInvokeSynchronousTestsDiscardingReturnedValues()
    {
        var output = await Run<SyncTestClass>();

        output.ShouldHaveResults(
            "SyncTestClass.ReturnsInteger passed",
            "SyncTestClass.ReturnsNull passed",
            "SyncTestClass.ReturnsObject passed",
            "SyncTestClass.ReturnsString passed");

        output.ShouldHaveLifecycle(
            "ReturnsInteger",
            "ReturnsNull",
            "ReturnsObject",
            "ReturnsString");
    }

    public async Task ShouldAwaitAsynchronousTestsToEnsureCompleteExecution()
    {
        var output = await Run<AsyncTestClass>();

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
            "AsyncTestClass.NullTask failed: The asynchronous method NullTask() returned null, " +
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
            "GenericAsyncValueTaskFail",
            "GenericAsyncValueTaskWithResult",
            "GenericTaskFail",
            "GenericTaskWithResult",
            "NullTask");
    }

    public async Task ShouldRunFSharpAsyncResultsToEnsureCompleteExecution()
    {
        var output = await Run<FSharpAsyncTestClass>();

        output.ShouldHaveResults(
            "FSharpAsyncTestClass.AsyncPass passed",
            "FSharpAsyncTestClass.FailBeforeAsync failed: 'FailBeforeAsync' failed!",
            "FSharpAsyncTestClass.FailFromAsync failed: result should be 0 but was 3",
            "FSharpAsyncTestClass.NullAsync failed: The asynchronous method NullAsync() returned null, " +
            "but a non-null awaitable object was expected.");

        output.ShouldHaveLifecycle(
            "AsyncPass",
            "FailBeforeAsync",
            "FailFromAsync",
            "NullAsync");
    }

    public async Task ShouldFailWithClearExplanationWhenAsyncTestReturnsNonStartedTask()
    {
        var output = await Run<FailDueToNonStartedTaskTestClass>();

        output.ShouldHaveResults(
            "FailDueToNonStartedTaskTestClass.Test failed: The method Test() returned a non-started task, which cannot " +
            "be awaited. Consider using Task.Run or Task.Factory.StartNew.");

        output.ShouldHaveLifecycle("Test");
    }

    public async Task ShouldFailUnsupportedReturnTypeDeclarationsRatherThanAttemptExecution()
    {
        var output = await Run<UnsupportedReturnTypeDeclarationsTestClass>();

        output.ShouldHaveResults(
            "UnsupportedReturnTypeDeclarationsTestClass.AsyncEnumerable failed: " +
            "The return type of method AsyncEnumerable() is an unsupported awaitable type. " +
            "To ensure the reliability of the test runner, declare " +
            "the method return type as `Task`, `Task<T>`, `ValueTask`, " +
            "or `ValueTask<T>`.",

            "UnsupportedReturnTypeDeclarationsTestClass.AsyncEnumerator failed: " +
            "The return type of method AsyncEnumerator() is an unsupported awaitable type. " +
            "To ensure the reliability of the test runner, declare " +
            "the method return type as `Task`, `Task<T>`, `ValueTask`, " +
            "or `ValueTask<T>`.",

            "UnsupportedReturnTypeDeclarationsTestClass.AsyncVoid failed: " +
            "The method AsyncVoid() is declared as `async void`, which is not supported. " +
            "To ensure the reliability of the test runner, declare " +
            "the method as `async Task`.",

            "UnsupportedReturnTypeDeclarationsTestClass.UntrustworthyAwaitable failed: " +
            "The return type of method UntrustworthyAwaitable() is an unsupported awaitable type. " +
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