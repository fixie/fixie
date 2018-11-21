namespace Fixie.Tests.Cases
{
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Assertions;
    using static System.Environment;
    using static Utility;

    public class AsyncCaseTests
    {
        public void ShouldAwaitThenPassUponSuccessfulAsyncExecution()
        {
            Run<AwaitThenPassTestClass>()
                .ShouldEqual(
                    For<AwaitThenPassTestClass>(".Test passed"));
        }

        public void ShouldAwaitResultThenPassUponSuccessfulAsyncExecution()
        {
            Run<AwaitResultThenPassTestClass>()
                .ShouldEqual(
                    For<AwaitResultThenPassTestClass>(".Test passed"));
        }

        public void ShouldCompleteTaskThenPassUponSuccessfulTaskExecution()
        {
            Run<CompleteTaskThenPassTestClass>()
                .ShouldEqual(
                    For<CompleteTaskThenPassTestClass>(".Test passed"));
        }

        public void ShouldGetTaskResultThenPassUponSuccessfulTaskExecution()
        {
            Run<CompleteTaskWithResultThenPassTestClass>()
                .ShouldEqual(
                    For<CompleteTaskWithResultThenPassTestClass>(".Test passed"));
        }

        public void ShouldPassForNullTask()
        {
            Run<NullTaskTestClass>()
                .ShouldEqual(
                    For<NullTaskTestClass>(".Test passed"));
        }

        public void ShouldFailWithOriginalExceptionWhenAsyncCaseMethodThrowsAfterAwaiting()
        {
            Run<AwaitThenFailTestClass>()
                .ShouldEqual(
                    For<AwaitThenFailTestClass>(
                        ".Test failed: Expected: 0" + NewLine +
                        "Actual:   3"));
        }

        public void ShouldFailWithOriginalExceptionWhenAsyncCaseMethodThrowsWithinTheAwaitedTask()
        {
            Run<AwaitOnTaskThatThrowsTestClass>()
                .ShouldEqual(
                    For<AwaitOnTaskThatThrowsTestClass>(".Test failed: Attempted to divide by zero."));
        }

        public void ShouldFailWithOriginalExceptionWhenAsyncCaseMethodThrowsBeforeAwaitingOnAnyTask()
        {
            Run<FailBeforeAwaitTestClass>()
                .ShouldEqual(
                    For<FailBeforeAwaitTestClass>(".Test failed: 'Test' failed!"));
        }

        public void ShouldFailUnsupportedAsyncVoidCases()
        {
            Run<UnsupportedAsyncVoidTestTestClass>()
                .ShouldEqual(
                    For<UnsupportedAsyncVoidTestTestClass>(".Test failed: " +
                        "Async void methods are not supported. Declare async methods with a return type of " +
                        "Task to ensure the task actually runs to completion."));
        }

        abstract class SampleTestClassBase
        {
            protected static void ThrowException([CallerMemberName] string member = null)
            {
                throw new FailureException(member);
            }

            protected static Task<int> Divide(int numerator, int denominator)
            {
                return Task.Run(() => numerator/denominator);
            }
        }

        class AwaitThenPassTestClass : SampleTestClassBase
        {
            public async Task Test()
            {
                var result = await Divide(15, 5);

                result.ShouldEqual(3);
            }
        }

        class AwaitResultThenPassTestClass : SampleTestClassBase
        {
            public async Task<bool> Test()
            {
                var result = await Divide(15, 5);

                result.ShouldEqual(3);

                return true;
            }
        }

        class CompleteTaskThenPassTestClass : SampleTestClassBase
        {
            public Task Test()
            {
                var divide = Divide(15, 5);

                return divide.ContinueWith(division =>
                {
                    division.Result.ShouldEqual(3);
                });
            }
        }

        class CompleteTaskWithResultThenPassTestClass : SampleTestClassBase
        {
            public Task<bool> Test()
            {
                var divide = Divide(15, 5);

                return divide.ContinueWith(division =>
                {
                    division.Result.ShouldEqual(3);

                    return true;
                });
            }
        }

        class NullTaskTestClass : SampleTestClassBase
        {
            public Task Test()
            {
                // Although unlikely, we must ensure that
                // we don't attempt to wait on a Task that
                // is in fact null.
                return null;
            }
        }

        class AwaitThenFailTestClass : SampleTestClassBase
        {
            public async Task Test()
            {
                var result = await Divide(15, 5);

                result.ShouldEqual(0);
            }
        }

        class AwaitOnTaskThatThrowsTestClass : SampleTestClassBase
        {
            public async Task Test()
            {
                await Divide(15, 0);

                throw new ShouldBeUnreachableException();
            }
        }

        class FailBeforeAwaitTestClass : SampleTestClassBase
        {
            public async Task Test()
            {
                ThrowException();

                await Divide(15, 5);
            }
        }

        class UnsupportedAsyncVoidTestTestClass : SampleTestClassBase
        {
            public async void Test()
            {
                await Divide(15, 5);

                throw new ShouldBeUnreachableException();
            }
        }
    }
}