namespace Fixie
{
    using System;

    /// <summary>
    /// Defines a behavior that can wrap each test case.
    /// 
    /// <para>
    /// The behavior may perform custom actions before, after, or instead of
    /// executing each test case. Invoke next() to proceed with normal execution.
    /// </para>
    /// </summary>
    public interface CaseBehavior : Behavior<Case>
    {
    }

    /// <summary>
    /// Defines a behavior that can wrap each test case.
    /// 
    /// <para>
    /// The behavior may perform custom actions before, after, or instead of
    /// executing each test case. Invoke next() to proceed with normal execution.
    /// </para>
    /// </summary>
    public delegate void CaseBehaviorAction(Case context, Action next);
}