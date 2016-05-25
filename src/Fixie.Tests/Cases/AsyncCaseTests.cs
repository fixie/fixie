namespace Fixie.Tests.Cases
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Should;

    public class AsyncCaseTests : CaseTests
    {
        public void ShouldPassUponSuccessfulAsyncExecution()
        {
            Run<AwaitThenPassTestClass>();

            Listener.Entries.ShouldEqual(
                For<AwaitThenPassTestClass>(".Test passed"));
        }

        public void ShouldFailWithOriginalExceptionWhenAsyncCaseMethodThrowsAfterAwaiting()
        {
            Run<AwaitThenFailTestClass>();

            Listener.Entries.ShouldEqual(
                For<AwaitThenFailTestClass>(".Test failed: Assert.Equal() Failure" + Environment.NewLine +
                "Expected: 0" + Environment.NewLine +
                "Actual:   3"));
        }

        public void ShouldFailWithOriginalExceptionWhenAsyncCaseMethodThrowsWithinTheAwaitedTask()
        {
            Run<AwaitOnTaskThatThrowsTestClass>();

            Listener.Entries.ShouldEqual(
                For<AwaitOnTaskThatThrowsTestClass>(".Test failed: Attempted to divide by zero."));
        }

        public void ShouldFailWithOriginalExceptionWhenAsyncCaseMethodThrowsBeforeAwaitingOnAnyTask()
        {
            Run<FailBeforeAwaitTestClass>();

            Listener.Entries.ShouldEqual(
                For<FailBeforeAwaitTestClass>(".Test failed: 'Test' failed!"));
        }

        public void ShouldFailUnsupportedAsyncVoidCases()
        {
            Run<UnsupportedAsyncVoidTestTestClass>();

            Listener.Entries.ShouldEqual(
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