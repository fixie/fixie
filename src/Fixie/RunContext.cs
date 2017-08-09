namespace Fixie
{
    using System;
    using System.Reflection;

    /// <summary>
    /// Describes the context in which a test run was initiated.
    /// </summary>
    static class RunContext
    {
        public static void Initialize()
        {
            TargetType = null;
            TargetMethod = null;
        }

        public static void Initialize(Type targetType)
        {
            TargetType = targetType;
            TargetMethod = null;
        }

        public static void Initialize(MethodInfo targetMethod)
        {
            TargetType = null;
            TargetMethod = targetMethod;
        }

        /// <summary>
        /// Gets the target Type identified by the test runner as the
        /// sole item to be executed. Null under normal test execution.
        /// </summary>
        public static Type TargetType { get; private set; }

        /// <summary>
        /// Gets the target MethodInfo identified by the test runner as the
        /// sole item to be executed. Null under normal test execution.
        /// </summary>
        public static MethodInfo TargetMethod { get; private set; }
    }
}