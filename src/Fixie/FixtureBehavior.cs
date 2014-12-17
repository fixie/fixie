using Fixie.Internal;

namespace Fixie
{
    /// <summary>
    /// Defines a behavior that can wrap each test fixture
    /// (test class instance). The behavior may perform custom
    /// actions before and/or after each test fixture executes.
    /// </summary>
    public interface FixtureBehavior : Behavior<Fixture>
    {
    }
}