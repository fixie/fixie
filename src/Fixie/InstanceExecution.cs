using System;
using System.Collections.Generic;
using Fixie.Execution;

namespace Fixie
{
    public class InstanceExecution : BehaviorContext
    {
        public InstanceExecution(Type testClass, object instance, IReadOnlyList<CaseExecution> caseExecutions)
        {
            TestClass = testClass;
            Instance = instance;
            CaseExecutions = caseExecutions;
        }

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