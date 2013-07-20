using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Fixie.Behaviors;
using Should;

namespace Fixie.Tests.Behaviors
{
    public class InvokeTests
    {
        bool invoked;
        readonly Invoke invoke = new Invoke();

        public void ShouldInvokeMethods()
        {
            var @case = Case("Returns");

            invoke.Execute(@case, this);

            invoked.ShouldBeTrue();

            @case.Exceptions.Count.ShouldEqual(0);
        }

        public void ShouldLogExceptionWhenMethodCannotBeInvoked()
        {
            var @case = Case("CannotInvoke");

            invoke.Execute(@case, this);

            invoked.ShouldBeFalse();

            ExpectException(@case, "TargetParameterCountException", "Parameter count mismatch.");
        }

        public void ShouldLogOriginalExceptionWhenMethodThrows()
        {
            var @case = Case("Throws");

            invoke.Execute(@case, this);

            invoked.ShouldBeTrue();

            ExpectException(@case, "FailureException", "'Throws' failed!");
        }

        public void ShouldInvokeAsyncMethods()
        {
            var @case = Case("Await");

            invoke.Execute(@case, this);

            invoked.ShouldBeTrue();

            @case.Exceptions.Count.ShouldEqual(0);
        }

        public void ShouldLogOriginalExceptionWhenAsyncMethodThrowsAfterAwaiting()
        {
            var @case = Case("AwaitThenThrow");

            invoke.Execute(@case, this);

            invoked.ShouldBeTrue();

            ExpectException(@case, "EqualException", "Assert.Equal() Failure" + Environment.NewLine +
                                                     "Expected: 0" + Environment.NewLine +
                                                     "Actual:   3");
        }

        public void ShouldLogOriginalExceptionWhenAsyncMethodThrowsWithinTheAwaitedTask()
        {
            var @case = Case("AwaitOnTaskThatThrows");

            invoke.Execute(@case, this);

            invoked.ShouldBeTrue();

            ExpectException(@case, "DivideByZeroException", "Attempted to divide by zero.");
        }

        public void ShouldLogOriginalExceptionWhenAsyncMethodThrowsBeforeAwaitingOnAnyTask()
        {
            var @case = Case("ThrowBeforeAwait");

            invoke.Execute(@case, this);

            invoked.ShouldBeTrue();

            ExpectException(@case, "FailureException", "'ThrowBeforeAwait' failed!");
        }

        public void ShouldLogExceptionWhenMethodIsUnsupportedAsyncVoid()
        {
            var @case = Case("UnsupportedAsyncVoid");

            invoke.Execute(@case, this);

            invoked.ShouldBeFalse();

            ExpectException(@case, "NotSupportedException",
                            "Async void methods are not supported. Declare async methods with a " +
                            "return type of Task to ensure the task actually runs to completion.");
        }

        static void ExpectException(Case @case, string expectedName, string expectedMessage)
        {
            var exception = @case.Exceptions.ToArray().Single();
            exception.GetType().Name.ShouldEqual(expectedName);
            exception.Message.ShouldEqual(expectedMessage);
        }

        static Case Case(string methodName)
        {
            var testClass = typeof(InvokeTests);
            return new Case(testClass, testClass.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic));
        }

        void Returns()
        {
            invoked = true;
        }

        void CannotInvoke(int argument)
        {
            invoked = true;
        }

        void Throws()
        {
            invoked = true;
            throw new FailureException();
        }

        static Task<int> Divide(int numerator, int denominator)
        {
            return Task.Run(() => numerator/denominator);
        }

        async Task Await()
        {
            invoked = true;

            var result = await Divide(15, 5);

            result.ShouldEqual(3);
        }

        async Task AwaitThenThrow()
        {
            invoked = true;

            var result = await Divide(15, 5);

            result.ShouldEqual(0);
        }

        async Task AwaitOnTaskThatThrows()
        {
            invoked = true;

            await Divide(15, 0);

            throw new ShouldBeUnreachableException();
        }

        async Task ThrowBeforeAwait()
        {
            invoked = true;

            ThrowException();

            await Divide(15, 5);
        }

        async void UnsupportedAsyncVoid()
        {
            invoked = true;

            await Divide(15, 5);

            throw new ShouldBeUnreachableException();
        }

        static void ThrowException([CallerMemberName] string member = null)
        {
            throw new FailureException(member);
        }
    }
}
