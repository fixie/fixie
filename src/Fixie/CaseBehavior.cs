using Fixie.Internal;

namespace Fixie
{
    /// <summary>
    /// Defines a behavior that can wrap each test case.
    /// The behavior may perform custom actions before
    /// and/or after each test case executes.
    /// </summary>
    public interface CaseBehavior : Behavior<Case>
    {
    }
}
