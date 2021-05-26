namespace Fixie.Tests
{
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Assertions;
    using Microsoft.FSharp.Control;
    using static System.Environment;

    public class AsyncCaseTests : InstrumentedExecutionTests
    {
        public async Task ShouldAwaitAsynchronousTestsToEnsureCompleteExecution()
        {
            var output = await RunAsync<AsyncTestClass>();

            output.ShouldHaveResults(
                "AsyncTestClass.AwaitTaskThenPass passed",
                "AsyncTestClass.AwaitValueTaskThenPass passed",
                "AsyncTestClass.CompleteTaskThenPass passed",
                "AsyncTestClass.FailAfterAwaitTask failed: Expected: 0" + NewLine + "Actual:   3",
                "AsyncTestClass.FailAfterAwaitValueTask failed: Expected: 0" + NewLine + "Actual:   3",
                "AsyncTestClass.FailBeforeAwaitTask failed: 'FailBeforeAwaitTask' failed!",
                "AsyncTestClass.FailBeforeAwaitValueTask failed: 'FailBeforeAwaitValueTask' failed!",
                "AsyncTestClass.FailDuringAwaitTask failed: Attempted to divide by zero.",
                "AsyncTestClass.FailDuringAwaitValueTask failed: Attempted to divide by zero.",
                "AsyncTestClass.NullTask failed: This asynchronous method returned null, " +
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
                "NullTask");
        }

        public async Task ShouldRunFSharpAsyncResultsToEnsureCompleteExecution()
        {
            var output = await RunAsync<FSharpAsyncTestClass>();

            output.ShouldHaveResults(
                "FSharpAsyncTestClass.AsyncPass passed",
                "FSharpAsyncTestClass.FailBeforeAsync failed: 'FailBeforeAsync' failed!",
                "FSharpAsyncTestClass.FailFromAsync failed: Expected: 0" + NewLine + "Actual:   3",
                "FSharpAsyncTestClass.NullAsync failed: This asynchronous method returned null, " +
                "but a non-null awaitable object was expected.");

            output.ShouldHaveLifecycle(
                "AsyncPass",
                "FailBeforeAsync",
                "FailFromAsync",
                "NullAsync");
        }

        public async Task ShouldFailWithClearExplanationWhenAsyncTestReturnsNonStartedTask()
        {
            var output = await RunAsync<FailDueToNonStartedTaskTestClass>();

            output.ShouldHaveResults(
                "FailDueToNonStartedTaskTestClass.Test failed: The test returned a non-started task, which cannot " +
                "be awaited. Consider using Task.Run or Task.Factory.StartNew.");

            output.ShouldHaveLifecycle("Test");
        }

        public async Task ShouldFailUnsupportedReturnTypeDeclarationsRatherThanAttemptExecution()
        {
            var output = await RunAsync<UnsupportedReturnTypeDeclarationsTestClass>();

            output.ShouldHaveResults(
                "UnsupportedReturnTypeDeclarationsTestClass.AsyncEnumerable failed: " +
                "Test method return type is not supported. Declare " +
                "the test method return type as `void`, `Task`, or `ValueTask`.",

                "UnsupportedReturnTypeDeclarationsTestClass.AsyncEnumerator failed: " +
                "Test method return type is not supported. Declare " +
                "the test method return type as `void`, `Task`, or `ValueTask`.",

                "UnsupportedReturnTypeDeclarationsTestClass.AsyncGenericTask failed: " +
                "Test method return type is not supported. Declare " +
                "the test method return type as `void`, `Task`, or `ValueTask`.",

                "UnsupportedReturnTypeDeclarationsTestClass.AsyncVoid failed: " +
                "`async void` test methods are not supported. Declare " +
                "the test method as `async Task` to ensure the task " +
                "actually runs to completion.",

                "UnsupportedReturnTypeDeclarationsTestClass.GenericTask failed: " +
                "Test method return type is not supported. Declare " +
                "the test method return type as `void`, `Task`, or `ValueTask`.",

                "UnsupportedReturnTypeDeclarationsTestClass.GenericValueTask failed: " +
                "Test method return type is not supported. Declare " +
                "the test method return type as `void`, `Task`, or `ValueTask`.",

                "UnsupportedReturnTypeDeclarationsTestClass.Object failed: " +
                "Test method return type is not supported. Declare " +
                "the test method return type as `void`, `Task`, or `ValueTask`.",

                "UnsupportedReturnTypeDeclarationsTestClass.UntrustworthyAwaitable failed: " +
                "Test method return type is not supported. Declare " +
                "the test method return type as `void`, `Task`, or `ValueTask`."
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
            public async Task<bool> AsyncGenericTask()
            {
                WhereAmI();

                await DivideAsync(15, 5);
                throw new ShouldBeUnreachableException();
            }

            public async void AsyncVoid()
            {
                WhereAmI();

                await DivideAsync(15, 5);
                throw new ShouldBeUnreachableException();
            }

            public Task<bool> GenericTask()
            {
                WhereAmI();

                DivideAsync(15, 5);
                throw new ShouldBeUnreachableException();
            }

            public async ValueTask<bool> GenericValueTask()
            {
                WhereAmI();

                await DivideAsync(15, 5);
                throw new ShouldBeUnreachableException();
            }

            public object Object()
            {
                WhereAmI();

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
}