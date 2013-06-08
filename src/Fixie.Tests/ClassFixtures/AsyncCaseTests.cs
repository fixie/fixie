using System;
using System.Threading.Tasks;
using Fixie.Conventions;
using Should;

namespace Fixie.Tests.ClassFixtures
{
    public class AsyncCaseTests
    {
        public void FailingTest()
        {
            1.ShouldEqual(2);
        }

        public void ShouldPassUponSuccessfulAsyncExecution()
        {
            var listener = new StubListener();

            new SelfTestConvention().Execute(listener, typeof(AwaitThenPassFixture));

            listener.ShouldHaveEntries(
                "Fixie.Tests.ClassFixtures.AsyncCaseTests+AwaitThenPassFixture.Test passed.");
        }

        public void ShouldFailWithOriginalExceptionWhenAsyncCaseMethodThrowsAfterAwaiting()
        {
            var listener = new StubListener();

            new SelfTestConvention().Execute(listener, typeof(AwaitThenFailFixture));

            listener.ShouldHaveEntries(
                "Fixie.Tests.ClassFixtures.AsyncCaseTests+AwaitThenFailFixture.Test failed: Assert.Equal() Failure" + Environment.NewLine +
                "Expected: 0" + Environment.NewLine +
                "Actual:   3");
        }

        public void ShouldFailWithOriginalExceptionWhenAsyncCaseMethodThrowsWithinTheAwaitedTask()
        {
            var listener = new StubListener();

            new SelfTestConvention().Execute(listener, typeof(AwaitOnTaskThatThrowsFixture));

            listener.ShouldHaveEntries(
                "Fixie.Tests.ClassFixtures.AsyncCaseTests+AwaitOnTaskThatThrowsFixture.Test failed: Attempted to divide by zero.");
        }

        public void ShouldFailWithOriginalExceptionWhenAsyncCaseMethodThrowsBeforeAwaitingOnAnyTask()
        {
            var listener = new StubListener();

            new SelfTestConvention().Execute(listener, typeof(FailBeforeAwaitFixture));

            listener.ShouldHaveEntries(
                "Fixie.Tests.ClassFixtures.AsyncCaseTests+FailBeforeAwaitFixture.Test failed: Exception of type " +
                "'Fixie.Tests.ClassFixtures.AsyncCaseTests+MethodInvokedException' was thrown.");
        }

        public void ShouldFailUnsupportedAsyncVoidCases()
        {
            var listener = new StubListener();

            new SelfTestConvention().Execute(listener, typeof(UnsupportedAsyncVoidFixture));

            listener.ShouldHaveEntries(
                "Fixie.Tests.ClassFixtures.AsyncCaseTests+UnsupportedAsyncVoidFixture.Test failed: " +
                "Async void methods are not supported. Declare async methods with a return type of " +
                "Task to ensure the task actually runs to completion.");
        }

        abstract class SampleFixtureBase
        {
            protected static void ThrowException()
            {
                throw new MethodInvokedException();
            }

            protected static Task<int> Divide(int numerator, int denominator)
            {
                return Task.Run(() => numerator/denominator);
            }
        }

        class AwaitThenPassFixture : SampleFixtureBase
        {
            public async Task Test()
            {
                var result = await Divide(15, 5);

                result.ShouldEqual(3);
            }
        }

        class AwaitThenFailFixture : SampleFixtureBase
        {
            public async Task Test()
            {
                var result = await Divide(15, 5);

                result.ShouldEqual(0);
            }
        }

        class AwaitOnTaskThatThrowsFixture : SampleFixtureBase
        {
            public async Task Test()
            {
                await Divide(15, 0);

                throw new ShouldBeUnreachableException();
            }
        }

        class FailBeforeAwaitFixture : SampleFixtureBase
        {
            public async Task Test()
            {
                ThrowException();

                await Divide(15, 5);
            }
        }

        class UnsupportedAsyncVoidFixture : SampleFixtureBase
        {
            public async void Test()
            {
                await Divide(15, 5);

                throw new ShouldBeUnreachableException();
            }
        }

        class MethodInvokedException : Exception { }
    }
}