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
        readonly List<string> log;
        readonly Type fixtureClass;
        readonly Convention convention;

        public ExecuteCasesTests()
        {
            log = new List<string>();
            fixtureClass = typeof(SampleFixture);

            convention = new SelfTestConvention();
            convention.CaseExecution.Wrap((method, instance, exceptions, inner) =>
            {
                log.Add(method.Name);
                inner.Execute(method, instance, exceptions);
            });
        }

        public void ShouldPerformCaseExecutionBehaviorForAllGivenCases()
        {
            var cases = new[]
            {
                new Case(fixtureClass, Method("PassingCase")), 
                new Case(fixtureClass, Method("FailingCase"))
            };

            var executeCases = new ExecuteCases();
            executeCases.Execute(fixtureClass, new SampleFixture(), cases, convention);

            cases[0].Exceptions.Any().ShouldBeFalse();
            cases[1].Exceptions.ToArray().Single().Message.ShouldEqual("Exception of type 'Fixie.Tests.FailureException' was thrown.");
            log.ShouldEqual("PassingCase", "FailingCase");
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