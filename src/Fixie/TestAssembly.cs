namespace Fixie
{
    using System.Collections.Generic;
    using System.Reflection;

    public class TestAssembly
    {
        internal TestAssembly(Assembly assembly)
        {
            Assembly = assembly;
            TestClasses = default!;
        }

        internal Assembly Assembly { get; }

        /// <summary>
        /// The test classes under execution.
        /// </summary>
        public IReadOnlyList<TestClass> TestClasses { get; internal set; } //TODO: Initialize at construction time, once TestClass no longer needs a back reference to its TestAssembly.
    }
}