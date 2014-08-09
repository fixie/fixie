using System;
using System.Collections.Generic;
using Fixie.Execution;

namespace Fixie
{
    public class Fixture : BehaviorContext
    {
        public Fixture(Class @class, object instance, IReadOnlyList<Case> cases)
        {
            Class = @class;
            Instance = instance;
            Cases = cases;
        }

        public Class Class { get; private set; }
        public object Instance { get; private set; }
        public IReadOnlyList<Case> Cases { get; private set; }

        public void Fail(Exception reason)
        {
            foreach (var @case in Cases)
                @case.Fail(reason);
        }
    }
}