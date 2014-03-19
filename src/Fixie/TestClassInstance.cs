using System;
using System.Collections.Generic;
using Fixie.Behaviors;

namespace Fixie
{
    public class TestClassInstance
    {
        public TestClassInstance(Type testClass, object instance, CaseBehavior caseExecutionBehavior, IReadOnlyList<CaseExecution> caseExecutions)
        {
            TestClass = testClass;
            Instance = instance;
            CaseExecutionBehavior = caseExecutionBehavior;
            CaseExecutions = caseExecutions;
        }

        public Type TestClass { get; private set; }
        public object Instance { get; private set; }
        public CaseBehavior CaseExecutionBehavior { get; private set; }
        public IReadOnlyList<CaseExecution> CaseExecutions { get; private set; }

        public void FailCases(Exception exception)
        {
            foreach (var caseExecution in CaseExecutions)
                caseExecution.Fail(exception);
        }
    }
}