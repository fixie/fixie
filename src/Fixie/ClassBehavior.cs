using Fixie.Internal;

namespace Fixie
{
    /// <summary>
    /// Defines a behavior that can wrap each test class.
    /// The behavior may perform custom actions before
    /// and/or after each test class executes.
    /// </summary>
    public interface ClassBehavior : Behavior<Class>
    {
    }
}