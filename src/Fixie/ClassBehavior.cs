using System;
using Fixie.Internal;

namespace Fixie
{
    /// <summary>
    /// Defines a behavior that can wrap each test class. The behavior may perform
    /// custom actions before, after, or instead of the inner behavior being wrapped.
    /// Invoke next() to proceed with normal execution.
    /// </summary>
    public interface ClassBehavior : Behavior<Class>
    {
    }

    /// <summary>
    /// Defines a behavior that can wrap each test class. The behavior may perform
    /// custom actions before, after, or instead of the inner behavior being wrapped.
    /// Invoke next() to proceed with normal execution.
    /// </summary>
    public delegate void ClassBehaviorAction(Class context, Action next);
}