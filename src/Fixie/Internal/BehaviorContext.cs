namespace Fixie.Internal
{
    using System;

    public interface BehaviorContext
    {
        void Fail(Exception reason);
    }
}