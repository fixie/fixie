namespace Fixie
{
    using System.Collections.Immutable;
    using System.Reflection;

    public class TestAssembly
    {
        internal TestAssembly(Assembly assembly, ImmutableHashSet<string> selectedTests)
        {
            Assembly = assembly;
            SelectedTests = selectedTests;
        }

        internal Assembly Assembly { get; }

        /// <summary>
        /// Gets the set of explicitly selected test names to be executed.
        /// Empty under normal test execution when all tests are being executed.
        /// </summary>
        public ImmutableHashSet<string> SelectedTests { get; }
    }
}