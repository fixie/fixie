namespace Fixie
{
    using System;
    using System.Reflection;

    /// <summary>
    /// Describes the context in which a test run was initiated.
    /// </summary>
    static class RunContext
    {
        public static void Set(string[] conventionArguments)
        {
            ConventionArguments = conventionArguments;
            TargetType = null;
            TargetMethod = null;
        }

        public static void Set(string[] conventionArguments, Type targetType)
        {
            ConventionArguments = conventionArguments;
            TargetType = targetType;
            TargetMethod = null;
        }

        public static void Set(string[] conventionArguments, MethodInfo targetMethod)
        {
            ConventionArguments = conventionArguments;
            TargetType = null;
            TargetMethod = targetMethod;
        }

        /// <summary>
        /// Gets the custom convention command line arguments provided by the test runner at the start of execution.
        /// </summary>
        public static string[] ConventionArguments { get; private set; }

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