using System;
using System.Collections.Generic;
using Fixie.Execution;

namespace Fixie
{
    public class ClassExecution : BehaviorContext
    {
        public ClassExecution(Type type, IReadOnlyList<Case> cases)
        {
            Type = type;
            Cases = cases;
        }

        public Type Type { get; private set; }
        public IReadOnlyList<Case> Cases { get; private set; }

        public void Fail(Exception reason)
        {
            foreach (var @case in Cases)
                @case.Fail(reason);
        }
    }
}