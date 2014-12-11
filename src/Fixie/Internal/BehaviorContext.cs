using System;

namespace Fixie.Internal
{
    public interface BehaviorContext
    {
        void Fail(Exception reason);
    }
}