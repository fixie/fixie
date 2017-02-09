namespace Fixie
{
    using System;

    /// <summary>
    /// Defines a behavior that can wrap each test fixture (test class instance).
    /// 
    /// <para>
    /// The behavior may perform custom actions before, after, or instead of
    /// executing each test fixture. Invoke next() to proceed with normal execution.
    /// </para>
    /// </summary>
    public interface FixtureBehavior : Behavior<Fixture>
    {
    }

    /// <summary>
    /// Defines a behavior that can wrap each test fixture (test class instance).
    /// 
    /// <para>
    /// The behavior may perform custom actions before, after, or instead of
    /// executing each test fixture. Invoke next() to proceed with normal execution.
    /// </para>
    /// </summary>
    public delegate void FixtureBehaviorAction(Fixture context, Action next);
}