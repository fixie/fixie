using System;
using System.Collections.Generic;
using Fixie.Conventions;

namespace Fixie
{
    public class TestClassInstance
    {
        readonly Convention convention;

        public TestClassInstance(Convention convention, Type testClass, object instance, IReadOnlyList<CaseExecution> caseExecutions)
        {
            TestClass = testClass;
            Instance = instance;
            this.convention = convention;
            CaseExecutions = caseExecutions;
        }

        public Type TestClass { get; private set; }
        public object Instance { get; private set; }
        public IReadOnlyList<CaseExecution> CaseExecutions { get; private set; }

        public void ExecuteCaseBehavior(CaseExecution caseExecution)
        {
            convention.CaseExecution.Behavior.Execute(caseExecution, Instance);
        }

        public void FailCases(Exception exception)
        {
            foreach (var caseExecution in CaseExecutions)
                caseExecution.Fail(exception);
        }
    }
}