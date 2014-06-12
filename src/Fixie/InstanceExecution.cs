using System;
using System.Collections.Generic;
using Fixie.Behaviors;

namespace Fixie
{
    public class InstanceExecution : BehaviorContext
    {
        public InstanceExecution(ExecutionModel executionModel, Type testClass, object instance, IReadOnlyList<CaseExecution> caseExecutions)
        {
            ExecutionModel = executionModel;
            TestClass = testClass;
            Instance = instance;
            CaseExecutions = caseExecutions;
        }

        public ExecutionModel ExecutionModel { get; private set; }
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