using System;
using System.Collections.Generic;

namespace Fixie
{
    public class InstanceExecution
    {
        public InstanceExecution(ExecutionPlan executionPlan, Type testClass, object instance, IReadOnlyList<CaseExecution> caseExecutions)
        {
            ExecutionPlan = executionPlan;
            TestClass = testClass;
            Instance = instance;
            CaseExecutions = caseExecutions;
        }

        public ExecutionPlan ExecutionPlan { get; private set; }
        public Type TestClass { get; private set; }
        public object Instance { get; private set; }
        public IReadOnlyList<CaseExecution> CaseExecutions { get; private set; }

        public void FailCases(Exception exception)
        {
            foreach (var caseExecution in CaseExecutions)
                caseExecution.Fail(exception);
        }
    }
}