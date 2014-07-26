using System;
using System.Collections.Generic;
using Fixie.Execution;

namespace Fixie
{
    public class ClassExecution : BehaviorContext
    {
        public ClassExecution(Type testClass, IReadOnlyList<Case> cases)
        {
            TestClass = testClass;
            Cases = cases;
        }

        public Type TestClass { get; private set; }
        public IReadOnlyList<Case> Cases { get; private set; }

        public void Fail(Exception reason)
        {
            foreach (var @case in Cases)
                @case.Fail(reason);
        }
    }
}