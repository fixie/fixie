using System;
using Fixie.Internal;

namespace Fixie
{
    /// <summary>
    /// Defines a behavior that can wrap each test fixture (test class instance).
    /// The behavior may perform custom actions before, after, or instead of the
    /// inner behavior being wrapped. Invoke next() to proceed with normal execution.
    /// </summary>
    public interface FixtureBehavior : Behavior<Fixture>
    {
    }

    /// <summary>
    /// Defines a behavior that can wrap each test fixture (test class instance).
    /// The behavior may perform custom actions before, after, or instead of the
    /// inner behavior being wrapped. Invoke next() to proceed with normal execution.
    /// </summary>
    public delegate void FixtureBehaviorAction(Fixture context, Action next);
}