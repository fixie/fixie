namespace Fixie
{
    using System.Reflection;

    /// <summary>
    /// Describes the context in which a test run was initiated.
    /// </summary>
    static class RunContext
    {
        public static void Initialize()
            => TargetMethod = null;

        public static void Initialize(MethodInfo targetMethod)
            => TargetMethod = targetMethod;

        /// <summary>
        /// Gets the target MethodInfo identified by the
        /// test runner as the sole item to be executed.
        /// Null under normal test execution.
        /// </summary>
        public static MethodInfo TargetMethod { get; private set; }
    }
}