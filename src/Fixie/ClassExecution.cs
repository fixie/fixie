using System;
using System.Collections.Generic;
using Fixie.Behaviors;

namespace Fixie
{
    public class ClassExecution : BehaviorContext
    {
        public ClassExecution(Type testClass, IReadOnlyList<CaseExecution> caseExecutions)
        {
            TestClass = testClass;
            CaseExecutions = caseExecutions;
        }

        public Type TestClass { get; private set; }
        public IReadOnlyList<CaseExecution> CaseExecutions { get; private set; }

        public void Fail(Exception reason)
        {
            foreach (var caseExecution in CaseExecutions)
                caseExecution.Fail(reason);
        }
    }
}