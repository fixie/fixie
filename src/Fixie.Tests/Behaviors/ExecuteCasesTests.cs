using System;
using System.Collections.Generic;
using System.Linq;
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
            convention.CaseExecution.Wrap((caseExecution, instance, innerBehavior) =>
            {
                log.Add(caseExecution.Case.Method.Name);
                innerBehavior();
            });
        }

        public void ShouldPerformCaseExecutionBehaviorForAllGivenCases()
        {
            var cases = new[]
            {
                new Case(testClass, testClass.GetInstanceMethod("Pass")),
                new Case(testClass, testClass.GetInstanceMethod("Fail"))
            };

            var caseExecutions = new[]
            {
                cases[0].Execution,
                cases[1].Execution
            };

            var executeCases = new ExecuteCases();
            var fixture = new Fixture(testClass, new SampleTestClass(), convention.CaseExecution.Behavior, caseExecutions);
            executeCases.Execute(fixture);

            caseExecutions[0].Exceptions.Any().ShouldBeFalse();
            caseExecutions[1].Exceptions.ToArray().Single().Message.ShouldEqual("'Fail' failed!");
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
    }
}