namespace Fixie
{
    using System.Reflection;

    /// <summary>
    /// Describes the context in which a test run was initiated.
    /// </summary>
    public class RunContext
    {
        internal RunContext() : this(null) { }

        internal RunContext(MethodInfo targetMethod)
            => TargetMethod = targetMethod;

        /// <summary>
        /// Gets the target MethodInfo identified by the
        /// test runner as the sole method to be executed.
        /// Null under normal test execution.
        /// </summary>
        public MethodInfo TargetMethod { get; }
    }
}