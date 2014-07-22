using System;

namespace Fixie.Execution
{
    public interface BehaviorContext
    {
        void Fail(Exception reason);
    }
}