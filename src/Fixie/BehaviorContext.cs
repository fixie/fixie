namespace Fixie
{
    using System;

    public interface BehaviorContext
    {
        void Fail(Exception reason);
    }
}