using System;
using System.Collections.Generic;
using Fixie.Behaviors;

namespace Fixie
{
    public class ClassExecution : BehaviorContext
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

        public void Fail(Exception reason)
        {
            foreach (var caseExecution in CaseExecutions)
                caseExecution.Fail(reason);
        }
    }
}