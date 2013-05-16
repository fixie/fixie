using System;
using System.Linq;
using System.Reflection;
using Should;

namespace Fixie.Tests
{
    public class InvokeTests
    {
        int invocationCount = 0;
        readonly ExceptionList exceptions = new ExceptionList();

        public void ShouldInvokeTheGivenMethod()
        {
            var invoke = new Invoke();
            invoke.Execute(Method("Returns"), this, exceptions);

            invocationCount.ShouldEqual(1);
            exceptions.Count.ShouldEqual(0);
        }

        public void ShouldLogExceptionWhenTheGivenMethodCannotBeInvoked()
        {
            var invoke = new Invoke();
            invoke.Execute(Method("CannotInvoke"), this, exceptions);
            
            invocationCount.ShouldEqual(0);
            exceptions.Count.ShouldEqual(1);

            var exception = exceptions.ToArray().Single();
            exception.GetType().Name.ShouldEqual("TargetParameterCountException");
            exception.Message.ShouldEqual("Parameter count mismatch.");
        }

        public void ShouldLogOriginalExceptionWhenTheGivenMethodThrows()
        {
            var invoke = new Invoke();
            invoke.Execute(Method("Throws"), this, exceptions);

            invocationCount.ShouldEqual(1);
            exceptions.Count.ShouldEqual(1);

            var exception = exceptions.ToArray().Single();
            exception.GetType().Name.ShouldEqual("FailureException");
            exception.Message.ShouldEqual("Exception of type 'Fixie.Tests.InvokeTests+FailureException' was thrown.");
        }

        void Returns()
        {
            invocationCount++;
        }

        void CannotInvoke(int argument)
        {
            invocationCount++;
        }

        void Throws()
        {
            invocationCount++;
            throw new FailureException();
        }

        class FailureException : Exception { }

        static MethodInfo Method(string name)
        {
            return typeof(InvokeTests).GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic);
        }
    }
}
