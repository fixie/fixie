namespace Fixie
{
    using System.Reflection;

    /// <summary>
    /// Defines a behavior to determine whether a given test method should be skipped during execution.
    /// Skipped test methods are never executed, but are counted and identified. When a test method is
    /// skipped, an optional reason string can be included in the test results.
    /// </summary>
    public interface SkipBehavior
    {
        /// <summary>
        /// Determines whether the given test method should be skipped during execution.
        /// </summary>
        bool SkipCase(MethodInfo testMethod);

        /// <summary>
        /// When a test method should be skipped during execution, returns an optional string to explain why.
        /// </summary>
        string GetSkipReason(MethodInfo testMethod);
    }
}