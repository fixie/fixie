namespace Fixie
{
    using System;

    /// <summary>
    /// Defines a test class lifecycle, to be executed once per test class.
    /// </summary>
    public interface Lifecycle
    {
        /// <summary>
        /// Executes a custom test class lifecycle for the given test class.
        /// </summary>
        void Execute(Type testClass, Action<CaseAction> runCases);
    }
}