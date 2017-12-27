namespace Fixie
{
    using System.Reflection;

    /// <summary>
    /// Describes the context in which a test run was initiated.
    /// </summary>
    static class RunContext
    {
        public static void Initialize()
            => TargetMember = null;

        public static void Initialize(MemberInfo targetMember)
            => TargetMember = targetMember;

        /// <summary>
        /// Gets the target Type or MethodInfo identified by
        /// the test runner as the sole item to be executed.
        /// Null under normal test execution.
        /// </summary>
        public static MemberInfo TargetMember { get; private set; }
    }
}