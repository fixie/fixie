using System;
using System.Collections.Generic;

namespace Fixie
{
    public class ClassExecution
    {
        public ClassExecution(ExecutionPlan executionPlan, Type testClass, IReadOnlyList<CaseExecution> caseExecutions)
        {
            ExecutionPlan = executionPlan;
            TestClass = testClass;
            CaseExecutions = caseExecutions;
        }

        public ExecutionPlan ExecutionPlan { get; private set; }
        public Type TestClass { get; private set; }
        public IReadOnlyList<CaseExecution> CaseExecutions { get; private set; }

        public void FailCases(Exception exception)
        {
            foreach (var caseExecution in CaseExecutions)
                caseExecution.Fail(exception);
        }
    }
}