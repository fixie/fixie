namespace Fixie
{
    using System;

    /// <summary>
    /// Defines a test class lifecycle, to be executed once per test class.
    /// </summary>
    public interface Lifecycle
    {
        /// <summary>
        /// Executes a test class lifecycle for the single test class.
        /// </summary>
        void Execute(RunContext runContext, Action<CaseAction> runCases);
    }
}