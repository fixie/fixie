using System;

namespace Fixie.Behaviors
{
    public interface BehaviorContext
    {
        void Fail(Exception reason);
    }
}