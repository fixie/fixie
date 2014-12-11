using System;
using System.Collections.Generic;
using Fixie.Execution;
using Fixie.Internal;

namespace Fixie
{
    public class Class : BehaviorContext
    {
        public Class(Type type, IReadOnlyList<Case> cases)
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