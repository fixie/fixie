namespace Fixie
{
    using System.Collections.Generic;
    using System.Reflection;

    public class TestAssembly
    {
        internal TestAssembly(Assembly assembly, IReadOnlyList<TestClass> testClasses)
        {
            Assembly = assembly;
            TestClasses = testClasses;
        }

        internal Assembly Assembly { get; }

        /// <summary>
        /// The test classes under execution.
        /// </summary>
        public IReadOnlyList<TestClass> TestClasses { get; }
    }
}