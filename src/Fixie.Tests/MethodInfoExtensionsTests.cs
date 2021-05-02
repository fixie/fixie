namespace Fixie.Tests
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Assertions;
    using static System.Environment;
    using static Utility;

    public class MethodInfoExtensionsTests : InstrumentedExecutionTests
    {
        class MethodInfoAccessingExecution : Execution
        {
            public async Task RunAsync(TestAssembly testAssembly)
            {
                foreach (var testClass in testAssembly.TestClasses)
                    foreach (var test in testAssembly.Tests)
                        foreach (var parameters in FromInputAttributes(test))
                        {
                            try
                            {
                                var instance = testClass.Construct();

                                await test.Method.CallAsync(instance, parameters);

                                await test.PassAsync(parameters);
                            }
                            catch (Exception exception)
                            {
                                await test.FailAsync(parameters, exception);
                            }
                        }
            }
        }

        public async Task ShouldCallSynchronousMethods()
        {
            var output = await RunAsync<SyncTestClass, MethodInfoAccessingExecution>();

            output.ShouldHaveResults(
                "SyncTestClass.Args(1, 2, 3) passed",
                "SyncTestClass.Args(1, 2, 3, \"Extra\") failed: Parameter count mismatch.",
                "SyncTestClass.Throws failed: 'Throws' failed!",
                "SyncTestClass.ZeroArgs passed");

            output.ShouldHaveLifecycle("Args", "Throws", "ZeroArgs");
        }

        public async Task ShouldResolveGenericTypeParametersWhenPossible()
        {
            var output = await RunAsync<GenericTestClass, MethodInfoAccessingExecution>();

            output.ShouldHaveResults(
                "GenericTestClass.Args<System.Int32, System.Int32>(1, 2, System.Int32, System.Int32) passed",
                "GenericTestClass.Args<System.Char, System.Double>('a', 3, System.Char, System.Double) passed",

                "GenericTestClass.ConstrainedArgs<System.Int32, System.Char>(1, 'a', System.Int32, System.Char) passed",
                "GenericTestClass.ConstrainedArgs<System.Int32, System.Char>(2, 'b', System.Int32, System.Int32) failed: Expected: System.Int32" + NewLine + "Actual:   System.Char",
                "GenericTestClass.ConstrainedArgs<T1, T2>(1, null, System.Int32, System.Object) failed: Could not resolve type parameters for generic method.",
                "GenericTestClass.ConstrainedArgs<T1, T2>(null, 2, System.Object, System.Int32) failed: Could not resolve type parameters for generic method.");

            output.ShouldHaveLifecycle("Args", "Args", "ConstrainedArgs", "ConstrainedArgs");
        }

        public async Task ShouldAwaitAsynchronousMethodsToEnsureCompleteExecution()
        {
            var output = await RunAsync<AsyncTestClass, MethodInfoAccessingExecution>();

            output.ShouldHaveResults(
                "AsyncTestClass.AwaitTaskThenPass passed",
                "AsyncTestClass.AwaitValueTaskThenPass passed",
                "AsyncTestClass.CompleteTaskThenPass passed",
                "AsyncTestClass.FailAfterAwaitTask failed: Expected: 0" + NewLine + "Actual:   3",
                "AsyncTestClass.FailAfterAwaitValueTask failed: Expected: 0" + NewLine + "Actual:   3",
                "AsyncTestClass.FailBeforeAwaitTask failed: 'FailBeforeAwaitTask' failed!",
                "AsyncTestClass.FailBeforeAwaitValueTask failed: 'FailBeforeAwaitValueTask' failed!",
                "AsyncTestClass.FailDuringAwaitTask failed: Attempted to divide by zero.",
                "AsyncTestClass.FailDuringAwaitValueTask failed: Attempted to divide by zero.");

            output.ShouldHaveLifecycle(
                "AwaitTaskThenPass",
                "AwaitValueTaskThenPass",
                "CompleteTaskThenPass",
                "FailAfterAwaitTask",
                "FailAfterAwaitValueTask",
                "FailBeforeAwaitTask",
                "FailBeforeAwaitValueTask",
                "FailDuringAwaitTask",
                "FailDuringAwaitValueTask");
        }

        public async Task ShouldThrowWithClearExplanationWhenMethodReturnsNullAwaitable()
        {
            var output = await RunAsync<NullTaskTestClass, MethodInfoAccessingExecution>();

            output.ShouldHaveResults(
                "NullTaskTestClass.Test failed: This asynchronous method returned null, " +
                "but a non-null awaitable object was expected.");

            output.ShouldHaveLifecycle("Test");
        }

        public async Task ShouldThrowWithClearExplanationWhenMethodReturnsNonStartedTask()
        {
            var output = await RunAsync<FailDueToNonStartedTaskTestClass, MethodInfoAccessingExecution>();

            output.ShouldHaveResults(
                "FailDueToNonStartedTaskTestClass.Test failed: The method returned a non-started task, which cannot " +
                "be awaited. Consider using Task.Run or Task.Factory.StartNew.");

            output.ShouldHaveLifecycle("Test");
        }

        public async Task ShouldThrowForUnsupportedReturnTypeDeclarationsRatherThanAttemptExecution()
        {
            var output = await RunAsync<UnsupportedReturnTypeDeclarationsTestClass, MethodInfoAccessingExecution>();

            output.ShouldHaveResults(
                "UnsupportedReturnTypeDeclarationsTestClass.AsyncGenericTask failed: " +
                "`async Task<T>` methods are not supported. Declare " +
                "the method as `async Task` to acknowledge that the " +
                "`Result` will not be witnessed.",

                "UnsupportedReturnTypeDeclarationsTestClass.AsyncVoid failed: " +
                "`async void` methods are not supported. Declare " +
                "the method as `async Task` to ensure the task " +
                "actually runs to completion.",

                "UnsupportedReturnTypeDeclarationsTestClass.GenericTask failed: " +
                "`Task<T>` methods are not supported. Declare " +
                "the method as `Task` to acknowledge that the " +
                "`Result` will not be witnessed.",

                "UnsupportedReturnTypeDeclarationsTestClass.GenericValueTask failed: " +
                "`async ValueTask<T>` methods are not supported. Declare " +
                "the method as `async ValueTask` to acknowledge that the " +
                "`Result` will not be witnessed.",

                "UnsupportedReturnTypeDeclarationsTestClass.Object failed: " +
                "Method return type is not supported. Declare " +
                "the method return type as `void`, `Task`, or `ValueTask`."
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

        class SyncTestClass
        {
            public void ZeroArgs() { WhereAmI(); }

            public void Throws() { WhereAmI(); ThrowException(); }

            [Input(1, 2, 3)]
            [Input(1, 2, 3, "Extra")]
            public void Args(int a, int b, int c) { WhereAmI(); }
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
        }

        class NullTaskTestClass
        {
            public Task? Test()
            {
                WhereAmI();

                // Although unlikely, we must ensure that
                // we don't attempt to wait on a Task that
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
        }
    }
}