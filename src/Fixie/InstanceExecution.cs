using System;
using System.Collections.Generic;
using Fixie.Behaviors;

namespace Fixie
{
    public class InstanceExecution : BehaviorContext
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

        public void Fail(Exception reason)
        {
            foreach (var caseExecution in CaseExecutions)
                caseExecution.Fail(reason);
        }
    }
}