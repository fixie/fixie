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
        readonly Type testClass;
        readonly Convention convention;

        public ExecuteCasesTests()
        {
            log = new List<string>();
            testClass = typeof(SampleFixture);

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
                new Case(testClass, Method("Pass")),
                new Case(testClass, Method("Fail"))
            };

            var executeCases = new ExecuteCases();
            var fixture = new Fixture(testClass, new SampleFixture(), convention.CaseExecution.Behavior, cases);
            executeCases.Execute(fixture);

            cases[0].Exceptions.Any().ShouldBeFalse();
            cases[1].Exceptions.ToArray().Single().Message.ShouldEqual("'Fail' failed!");
            log.ShouldEqual("Pass", "Fail");
        }

        private class SampleFixture
        {
            public void Pass() { }
            
            public void Fail()
            {
                throw new FailureException();
            }

            public void Ignored()
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