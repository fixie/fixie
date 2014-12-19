using System.Reflection;

namespace Fixie
{
    /// <summary>
    /// Describes the context in which a test run was initiated.
    /// </summary>
    public class RunContext
    {
        public static RunContext Current { get; private set; }

        public RunContext(Assembly assembly, Options options)
            : this(assembly, options, null) { }

        public RunContext(Assembly assembly, Options options, MemberInfo targetMember)
        {
            Assembly = assembly;
            Options = options;
            TargetMember = targetMember;

            Current = this;
        }

        /// <summary>
        /// Gets the test assembly being executed.
        /// </summary>
        public Assembly Assembly { get; private set; }

        /// <summary>
        /// Gets the custom Options set provided by the test runner at the start of execution.
        /// </summary>
        public Options Options { get; private set; }

        /// <summary>
        /// Gets the target Type or MethodInfo identified by
        /// the test runner as the sole item to be executed.
        /// Null under normal test execution.
        /// </summary>
        public MemberInfo TargetMember { get; private set; }
    }
}