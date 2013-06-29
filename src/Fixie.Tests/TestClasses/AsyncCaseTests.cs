using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Fixie.Conventions;
using Should;

namespace Fixie.Tests.TestClasses
{
    public class AsyncCaseTests
    {
        public void ShouldPassUponSuccessfulAsyncExecution()
        {
            var listener = new StubListener();

            new SelfTestConvention().Execute(listener, typeof(AwaitThenPassTestClass));

            listener.ShouldHaveEntries(
                "Fixie.Tests.TestClasses.AsyncCaseTests+AwaitThenPassTestClass.Test passed.");
        }

        public void ShouldFailWithOriginalExceptionWhenAsyncCaseMethodThrowsAfterAwaiting()
        {
            var listener = new StubListener();

            new SelfTestConvention().Execute(listener, typeof(AwaitThenFailTestClass));

            listener.ShouldHaveEntries(
                "Fixie.Tests.TestClasses.AsyncCaseTests+AwaitThenFailTestClass.Test failed: Assert.Equal() Failure" + Environment.NewLine +
                "Expected: 0" + Environment.NewLine +
                "Actual:   3");
        }

        public void ShouldFailWithOriginalExceptionWhenAsyncCaseMethodThrowsWithinTheAwaitedTask()
        {
            var listener = new StubListener();

            new SelfTestConvention().Execute(listener, typeof(AwaitOnTaskThatThrowsTestClass));

            listener.ShouldHaveEntries(
                "Fixie.Tests.TestClasses.AsyncCaseTests+AwaitOnTaskThatThrowsTestClass.Test failed: Attempted to divide by zero.");
        }

        public void ShouldFailWithOriginalExceptionWhenAsyncCaseMethodThrowsBeforeAwaitingOnAnyTask()
        {
            var listener = new StubListener();

            new SelfTestConvention().Execute(listener, typeof(FailBeforeAwaitTestClass));

            listener.ShouldHaveEntries(
                "Fixie.Tests.TestClasses.AsyncCaseTests+FailBeforeAwaitTestClass.Test failed: 'Test' failed!");
        }

        public void ShouldFailUnsupportedAsyncVoidCases()
        {
            var listener = new StubListener();

            new SelfTestConvention().Execute(listener, typeof(UnsupportedAsyncVoidTestTestClass));

            listener.ShouldHaveEntries(
                "Fixie.Tests.TestClasses.AsyncCaseTests+UnsupportedAsyncVoidTestTestClass.Test failed: " +
                "Async void methods are not supported. Declare async methods with a return type of " +
                "Task to ensure the task actually runs to completion.");
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