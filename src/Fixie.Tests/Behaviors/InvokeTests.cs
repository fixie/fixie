using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Fixie.Behaviors;
using Should;

namespace Fixie.Tests.Behaviors
{
    public class InvokeTests
    {
        bool invoked;
        readonly ExceptionList exceptions = new ExceptionList();
        readonly Invoke invoke = new Invoke();

        public void ShouldInvokeMethods()
        {
            invoke.Execute(Method("Returns"), this, exceptions);

            invoked.ShouldBeTrue();

            exceptions.Count.ShouldEqual(0);
        }

        public void ShouldLogExceptionWhenMethodCannotBeInvoked()
        {
            invoke.Execute(Method("CannotInvoke"), this, exceptions);

            invoked.ShouldBeFalse();

            ExpectException("TargetParameterCountException", "Parameter count mismatch.");
        }

        public void ShouldLogOriginalExceptionWhenMethodThrows()
        {
            invoke.Execute(Method("Throws"), this, exceptions);

            invoked.ShouldBeTrue();

            ExpectException("FailureException", "Exception of type 'Fixie.Tests.Behaviors.InvokeTests+FailureException' was thrown.");
        }

        public void ShouldInvokeAsyncMethods()
        {
            invoke.Execute(Method("Await"), this, exceptions);

            invoked.ShouldBeTrue();

            exceptions.Count.ShouldEqual(0);
        }

        public void ShouldLogOriginalExceptionWhenAsyncMethodThrowsAfterAwaiting()
        {
            invoke.Execute(Method("AwaitThenThrow"), this, exceptions);

            invoked.ShouldBeTrue();

            ExpectException("EqualException", "Assert.Equal() Failure" + Environment.NewLine +
                                              "Expected: 0" + Environment.NewLine +
                                              "Actual:   3");
        }

        public void ShouldLogOriginalExceptionWhenAsyncMethodThrowsWithinTheAwaitedTask()
        {
            invoke.Execute(Method("AwaitOnTaskThatThrows"), this, exceptions);

            invoked.ShouldBeTrue();

            ExpectException("DivideByZeroException", "Attempted to divide by zero.");
        }

        public void ShouldLogOriginalExceptionWhenAsyncMethodThrowsBeforeAwaitingOnAnyTask()
        {
            invoke.Execute(Method("ThrowBeforeAwait"), this, exceptions);

            invoked.ShouldBeTrue();

            ExpectException("FailureException", "Exception of type 'Fixie.Tests.Behaviors.InvokeTests+FailureException' was thrown.");
        }

        public void ShouldLogExceptionWhenMethodIsUnsupportedAsyncVoid()
        {
            invoke.Execute(Method("UnsupportedAsyncVoid"), this, exceptions);

            invoked.ShouldBeFalse();

            ExpectException("NotSupportedException",
                            "Async void methods are not supported. Declare async methods with a " +
                            "return type of Task to ensure the task actually runs to completion.");
        }

        void ExpectException(string expectedName, string expectedMessage)
        {
            var exception = exceptions.ToArray().Single();
            exception.GetType().Name.ShouldEqual(expectedName);
            exception.Message.ShouldEqual(expectedMessage);
        }

        static MethodInfo Method(string name)
        {
            return typeof(InvokeTests).GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic);
        }

        class FailureException : Exception { }

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

        static void ThrowException()
        {
            throw new FailureException();
        }
    }
}
