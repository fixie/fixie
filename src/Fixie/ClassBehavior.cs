namespace Fixie
{
    using System;
    using Internal;

    /// <summary>
    /// Defines a behavior that can wrap each test class.
    /// 
    /// <para>
    /// The behavior may perform custom actions before, after, or instead of
    /// executing each test class. Invoke next() to proceed with normal execution.
    /// </para>
    /// </summary>
    public interface ClassBehavior : Behavior<Class>
    {
    }

    /// <summary>
    /// Defines a behavior that can wrap each test class.
    /// 
    /// <para>
    /// The behavior may perform custom actions before, after, or instead of
    /// executing each test class. Invoke next() to proceed with normal execution.
    /// </para>
    /// </summary>
    public delegate void ClassBehaviorAction(Class context, Action next);
}