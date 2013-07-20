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
            testClass = typeof(SampleTestClass);

            convention = new SelfTestConvention();
            convention.CaseExecution.Wrap((@case, instance, inner) =>
            {
                log.Add(@case.Method.Name);
                inner.Execute(@case, instance);
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
            var fixture = new Fixture(testClass, new SampleTestClass(), convention.CaseExecution.Behavior, cases);
            executeCases.Execute(fixture);

            cases[0].Exceptions.Any().ShouldBeFalse();
            cases[1].Exceptions.ToArray().Single().Message.ShouldEqual("'Fail' failed!");
            log.ShouldEqual("Pass", "Fail");
        }

        private class SampleTestClass
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
            return typeof(SampleTestClass).GetMethod(name, BindingFlags.Instance | BindingFlags.Public);
        }
    }
}