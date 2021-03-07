namespace Fixie
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Reflection;

    public class TestAssembly
    {
        internal TestAssembly(Assembly assembly, ImmutableHashSet<string> selectedTests)
        {
            Assembly = assembly;
            SelectedTests = selectedTests;
            TestClasses = default!;
        }

        internal Assembly Assembly { get; }

        /// <summary>
        /// Gets the set of explicitly selected test names to be executed.
        /// Empty under normal test execution when all tests are being executed.
        /// </summary>
        public ImmutableHashSet<string> SelectedTests { get; }

        /// <summary>
        /// The test classes under execution.
        /// </summary>
        public IReadOnlyList<TestClass> TestClasses { get; internal set; } //TODO: Initialize at construction time, once TestClass no longer needs a back reference to its TestAssembly.
    }
}