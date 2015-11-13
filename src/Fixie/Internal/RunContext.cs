using System.Reflection;

namespace Fixie.Internal
{
    /// <summary>
    /// Describes the context in which a test run was initiated.
    /// </summary>
    static class RunContext
    {
        public static void Set(Options options)
        {
            Set(options, null);
        }

        public static void Set(Options options, MemberInfo targetMember)
        {
            Options = options ?? new Options();
            TargetMember = targetMember;
        }

        /// <summary>
        /// Gets the custom Options set provided by the test runner at the start of execution.
        /// </summary>
        public static Options Options { get; private set; }

        /// <summary>
        /// Gets the target Type or MethodInfo identified by
        /// the test runner as the sole item to be executed.
        /// Null under normal test execution.
        /// </summary>
        public static MemberInfo TargetMember { get; private set; }
    }
}