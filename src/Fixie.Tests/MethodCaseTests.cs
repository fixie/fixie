using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Should;

namespace Fixie.Tests
{
    public class MethodCaseTests
    {
        public void ShouldPassUponSuccessfulExecution()
        {
            ExecutionLog<SampleFixture>("Pass")
                .ShouldEqual("Fixie.Tests.MethodCaseTests+SampleFixture.Pass passed.");
        }

        public void ShouldFailWhenCaseMethodCannotBeInvoked()
        {
            ExecutionLog<SampleFixture>("CannotInvoke")
                .ShouldEqual("Fixie.Tests.MethodCaseTests+SampleFixture.CannotInvoke failed: Parameter count mismatch.");
        }

        public void ShouldFailWithOriginalExceptionWhenCaseMethodThrows()
        {
            ExecutionLog<SampleFixture>("Fail")
                .ShouldEqual("Fixie.Tests.MethodCaseTests+SampleFixture.Fail failed: Exception of type " +
                             "'Fixie.Tests.MethodCaseTests+MethodInvokedException' was thrown.");
        }

        public void ShouldPassUponSuccessfulAsyncExecution()
        {
            ExecutionLog<SampleFixture>("AwaitThenPass")
                .ShouldEqual("Fixie.Tests.MethodCaseTests+SampleFixture.AwaitThenPass passed.");
        }

        public void ShouldFailWithOriginalExceptionWhenAsyncCaseMethodThrowsAfterAwaiting()
        {
            ExecutionLog<SampleFixture>("AwaitThenFail")
                .ShouldEqual("Fixie.Tests.MethodCaseTests+SampleFixture.AwaitThenFail failed: Assert.Equal() Failure" + Environment.NewLine +
                             "Expected: 0" + Environment.NewLine +
                             "Actual:   3");
        }

        public void ShouldFailWithOriginalExceptionWhenAsyncCaseMethodThrowsWithinTheAwaitedTask()
        {
            ExecutionLog<SampleFixture>("AwaitOnTaskThatThrows")
                .ShouldEqual("Fixie.Tests.MethodCaseTests+SampleFixture.AwaitOnTaskThatThrows failed: Attempted to divide by zero.");
        }

        public void ShouldFailWithOriginalExceptionWhenAsyncCaseMethodThrowsBeforeAwaitingOnAnyTask()
        {
            ExecutionLog<SampleFixture>("FailBeforeAwait")
                .ShouldEqual("Fixie.Tests.MethodCaseTests+SampleFixture.FailBeforeAwait failed: Exception of type " +
                             "'Fixie.Tests.MethodCaseTests+MethodInvokedException' was thrown.");
        }

        public void ShouldFailUnsupportedAsyncVoidMethodCases()
        {
            ExecutionLog<SampleFixture>("UnsupportedAsyncVoid")
                .ShouldEqual("Fixie.Tests.MethodCaseTests+SampleFixture.UnsupportedAsyncVoid failed: Async void tests are not " +
                             "supported.  Declare async test methods with a return type of Task to ensure the task actually " +
                             "runs to completion.");
        }

        static IEnumerable<string> ExecutionLog<T>(string methodName) where T : new()
        {
            var listener = new StubListener();
            var fixtureClass = typeof(T);
            var fixture = new ClassFixture(fixtureClass, null, new T());

            var method = fixtureClass.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);

            var @case = new MethodCase(fixture, method);

            @case.Execute(listener);

            return listener.Entries;
        }

        class SampleFixture
        {
            public void Pass()
            {
            }

            public void Fail()
            {
                throw new MethodInvokedException();
            }

            public void CannotInvoke(int argument)
            {
            }

            public async Task AwaitThenPass()
            {
                var result = await Divide(15, 5);

                result.ShouldEqual(3);
            }

            public async Task AwaitThenFail()
            {
                var result = await Divide(15, 5);

                result.ShouldEqual(0);
            }

            public async Task AwaitOnTaskThatThrows()
            {
                await Divide(15, 0);

                throw new ShouldBeUnreachableException();
            }

            public async Task FailBeforeAwait()
            {
                ThrowException();

                await Divide(15, 5);
            }

            public async void UnsupportedAsyncVoid()
            {
                await Divide(15, 5);

                throw new ShouldBeUnreachableException();
            }

            static void ThrowException()
            {
                throw new MethodInvokedException();
            }

            static Task<int> Divide(int numerator, int denominator)
            {
                return Task.Run(() => numerator / denominator);
            }
        }

        class MethodInvokedException : Exception { }
    }
}