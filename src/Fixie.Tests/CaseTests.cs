using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Should;

namespace Fixie.Tests
{
    public class CaseTests
    {
        bool invoked;

        public void ShouldBeNamedAfterTheUnderlyingMethod()
        {
            var @case = Case("Returns");

            @case.Name.ShouldEqual("Fixie.Tests.CaseTests.Returns");
        }

        public void ShouldIncludeParmeterValuesInNameWhenTheUnderlyingMethodHasParameters()
        {
            var @case = Case("Parameterized", 123, true, 'a', "with \"quotes\"", "long \"string\" gets truncated", null, this);

            @case.Name.ShouldEqual("Fixie.Tests.CaseTests.Parameterized(123, True, 'a', \"with \\\"quotes\\\"\", \"long \\\"string\\\" g\"..., null, Fixie.Tests.CaseTests)");
        }

        public void ShouldInvokeMethods()
        {
            var @case = Case("Returns");

            @case.Execute(this);

            invoked.ShouldBeTrue();

            @case.Result.Exceptions.Count.ShouldEqual(0);
        }

        public void ShouldInvokeMethodsWithParameters()
        {
            var @case = Case("Parameterized", 123, true, 'a', "s1", "s2", null, this);

            @case.Execute(this);

            invoked.ShouldBeTrue();

            @case.Result.Exceptions.Count.ShouldEqual(0);
        }

        public void ShouldLogExceptionWhenMethodCannotBeInvoked()
        {
            var @case = Case("CannotInvoke");

            @case.Execute(this);

            invoked.ShouldBeFalse();

            ExpectException(@case, "TargetParameterCountException", "Parameter count mismatch.");
        }

        public void ShouldLogOriginalExceptionWhenMethodThrows()
        {
            var @case = Case("Throws");

            @case.Execute(this);

            invoked.ShouldBeTrue();

            ExpectException(@case, "FailureException", "'Throws' failed!");
        }

        public void ShouldInvokeAsyncMethods()
        {
            var @case = Case("Await");

            @case.Execute(this);

            invoked.ShouldBeTrue();

            @case.Result.Exceptions.Count.ShouldEqual(0);
        }

        public void ShouldLogOriginalExceptionWhenAsyncMethodThrowsAfterAwaiting()
        {
            var @case = Case("AwaitThenThrow");

            @case.Execute(this);

            invoked.ShouldBeTrue();

            ExpectException(@case, "EqualException", "Assert.Equal() Failure" + Environment.NewLine +
                                                     "Expected: 0" + Environment.NewLine +
                                                     "Actual:   3");
        }

        public void ShouldLogOriginalExceptionWhenAsyncMethodThrowsWithinTheAwaitedTask()
        {
            var @case = Case("AwaitOnTaskThatThrows");

            @case.Execute(this);

            invoked.ShouldBeTrue();

            ExpectException(@case, "DivideByZeroException", "Attempted to divide by zero.");
        }

        public void ShouldLogOriginalExceptionWhenAsyncMethodThrowsBeforeAwaitingOnAnyTask()
        {
            var @case = Case("ThrowBeforeAwait");

            @case.Execute(this);

            invoked.ShouldBeTrue();

            ExpectException(@case, "FailureException", "'ThrowBeforeAwait' failed!");
        }

        public void ShouldLogExceptionWhenMethodIsUnsupportedAsyncVoid()
        {
            var @case = Case("UnsupportedAsyncVoid");

            @case.Execute(this);

            invoked.ShouldBeFalse();

            ExpectException(@case, "NotSupportedException",
                            "Async void methods are not supported. Declare async methods with a " +
                            "return type of Task to ensure the task actually runs to completion.");
        }

        static void ExpectException(Case @case, string expectedName, string expectedMessage)
        {
            var exception = @case.Result.Exceptions.ToArray().Single();
            exception.GetType().Name.ShouldEqual(expectedName);
            exception.Message.ShouldEqual(expectedMessage);
        }

        static Case Case(string methodName, params object[] parameters)
        {
            var testClass = typeof(CaseTests);
            return new Case(testClass, testClass.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic), parameters);
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

        void Parameterized(int i, bool b, char ch, string s1, string s2, object obj, CaseTests complex)
        {
            invoked = true;
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
