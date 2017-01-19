namespace Fixie.Internal
{
    using System.Reflection;

    /// <summary>
    /// Describes the context in which a test run was initiated.
    /// </summary>
    static class RunContext
    {
        public static void Set(string[] arguments)
        {
            Set(arguments, null);
        }

        public static void Set(string[] arguments, MemberInfo targetMember)
        {
            Arguments = arguments;
            TargetMember = targetMember;
        }

        /// <summary>
        /// Gets the command line arguments provided by the test runner at the start of execution.
        /// </summary>
        public static string[] Arguments { get; private set; }

        /// <summary>
        /// Gets the target Type or MethodInfo identified by
        /// the test runner as the sole item to be executed.
        /// Null under normal test execution.
        /// </summary>
        public static MemberInfo TargetMember { get; private set; }
    }
}