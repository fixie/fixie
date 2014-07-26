using System;
using System.Collections.Generic;
using Fixie.Execution;

namespace Fixie
{
    public class InstanceExecution : BehaviorContext
    {
        public InstanceExecution(Type testClass, object instance, IReadOnlyList<Case> cases)
        {
            TestClass = testClass;
            Instance = instance;
            Cases = cases;
        }

        public Type TestClass { get; private set; }
        public object Instance { get; private set; }
        public IReadOnlyList<Case> Cases { get; private set; }

        public void Fail(Exception reason)
        {
            foreach (var @case in Cases)
                @case.Fail(reason);
        }
    }
}