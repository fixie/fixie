using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fixie.Behaviors;
using Fixie.Conventions;
using Should;

namespace Fixie.Tests.Behaviors
{
    public class ExecuteCasesTests
    {
        readonly Type fixtureClass;
        readonly SampleFixture instance;
        readonly LogMethod logMethod;
        readonly Convention convention;

        public ExecuteCasesTests()
        {
            fixtureClass = typeof(SampleFixture);
            instance = new SampleFixture();

            convention = new SelfTestConvention();
            logMethod = new LogMethod(convention.CaseExecutionBehavior);
            convention.CaseExecutionBehavior = logMethod;
        }

        public void ShouldPerformCaseExecutionBehaviorForAllGivenCases()
        {
            var cases = new[]
            {
                new Case(fixtureClass, Method("PassingCase")), 
                new Case(fixtureClass, Method("FailingCase"))
            };

            var executeCases = new ExecuteCases();
            executeCases.Execute(fixtureClass, instance, cases, convention);

            cases[0].Exceptions.Any().ShouldBeFalse();
            cases[1].Exceptions.ToArray().Single().Message.ShouldEqual("Exception of type 'Fixie.Tests.FailureException' was thrown.");
            logMethod.Log.ShouldEqual("PassingCase", "FailingCase");
        }

        private class LogMethod : MethodBehavior
        {
            readonly MethodBehavior inner;

            public LogMethod(MethodBehavior inner)
            {
                this.inner = inner;
                Log = new List<string>();
            }

            public List<string> Log { get; private set; }

            public void Execute(MethodInfo method, object instance, ExceptionList exceptions)
            {
                Log.Add(method.Name);
                inner.Execute(method, instance, exceptions);
            }
        }

        private class SampleFixture
        {
            public void PassingCase() { }
            
            public void FailingCase()
            {
                throw new FailureException();
            }

            public void IgnoredCase()
            {
                throw new ShouldBeUnreachableException();
            }
        }

        static MethodInfo Method(string name)
        {
            return typeof(SampleFixture).GetMethod(name, BindingFlags.Instance | BindingFlags.Public);
        }
    }
}