namespace Fixie
{
    /// <summary>
    /// Defines a behavior to determine wehther a given test case should be skipped during execution.
    /// Skipped test cases are never executed, but are counted and identified. When a test is skipped,
    /// an optional reason string can be included in the test results.
    /// </summary>
    public abstract class SkipBehavior
    {
        /// <summary>
        /// Determines whether the given test case should be skipped during execution.
        /// </summary>
        public abstract bool SkipCase(Case @case);

        /// <summary>
        /// When a test case should be skipped during execution, returns an optional string to explain why.
        /// </summary>
        public abstract string GetSkipReason(Case @case);
    }
}