using System;
using Fixie.Internal;

namespace Fixie
{
    /// <summary>
    /// Defines a behavior that can wrap each test case. The behavior may perform
    /// custom actions before, after, or instead of the inner behavior being wrapped.
    /// Invoke next() to proceed with normal execution.
    /// </summary>
    public interface CaseBehavior : Behavior<Case>
    {
    }

    /// <summary>
    /// Defines a behavior that can wrap each test case. The behavior may perform
    /// custom actions before, after, or instead of the inner behavior being wrapped.
    /// Invoke next() to proceed with normal execution.
    /// </summary>
    public delegate void CaseBehaviorAction(Case context, Action next);
}