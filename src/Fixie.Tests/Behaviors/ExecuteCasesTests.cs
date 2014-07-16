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
        static readonly List<string> log = new List<string>();
        readonly Type testClass;
        readonly Convention convention;

        class LogMethodName : CaseBehavior
        {
            public void Execute(CaseExecution caseExecution, Action next)
            {
                log.Add(caseExecution.Case.Method.Name);
                next();
            }
        }

        public ExecuteCasesTests()
        {
            log.Clear();
            testClass = typeof(SampleTestClass);

            convention = SelfTestConvention.Build();
            convention.CaseExecution.Wrap<LogMethodName>();
        }

        public void ShouldPerformCaseExecutionBehaviorForAllGivenCases()
        {
            var caseA = new Case(testClass.GetInstanceMethod("Pass"));
            var caseB = new Case(testClass.GetInstanceMethod("Fail"));

            var caseExecutions = new[]
            {
                new CaseExecution(caseA),
                new CaseExecution(caseB)
            };

            var executionPlan = new ExecutionPlan(convention.Config);
            var executeCases = new ExecuteCases(executionPlan);
            var instanceExecution = new InstanceExecution(testClass, new SampleTestClass(), caseExecutions);

            bool invokedNext = false;
            executeCases.Execute(instanceExecution, () => invokedNext = true);

            caseExecutions[0].Exceptions.Any().ShouldBeFalse();
            caseExecutions[1].Exceptions.Single().Message.ShouldEqual("'Fail' failed!");
            log.ShouldEqual("Pass", "Fail");
            invokedNext.ShouldBeFalse();
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