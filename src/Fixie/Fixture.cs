using System;
using System.Collections.Generic;
using Fixie.Internal;

namespace Fixie
{
    public class Fixture : BehaviorContext
    {
        public Fixture(Class testClass, object instance, IReadOnlyList<Case> cases)
        {
            Class = testClass;
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