using System;
using System.Reflection;
using NUnit.Framework;
using Shouldly;

namespace Fixie.Tests
{
    [TestFixture]
    public class CaseTests
    {
        [Test]
        public void ShouldInvokeTheGivenMethodWhenExecuted()
        {
            var fixtureClass = typeof(SampleFixture);
            var method = fixtureClass.GetMethod("Method", BindingFlags.Public | BindingFlags.Instance);

            var c = new Case(fixtureClass, method);

            bool threw = false;

            try
            {
                c.Execute();
            }
            catch (TargetInvocationException expected)
            {
                threw = true;
                expected.InnerException.ShouldBeTypeOf<MethodInvokedException>();
            }

            threw.ShouldBe(true);
        }

        class MethodInvokedException : Exception
        {
        }

        class SampleFixture
        {
            public void Method()
            {
                throw new MethodInvokedException();
            }
        }
    }
}