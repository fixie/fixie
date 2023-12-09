using System.Collections.Generic;
using System.Linq;

namespace Fixie;

public class TestSuite
{
    internal TestSuite(IReadOnlyList<TestClass> testClasses)
    {
        TestClasses = testClasses;
    }

    /// <summary>
    /// The test classes under execution.
    /// </summary>
    public IReadOnlyList<TestClass> TestClasses { get; }

    /// <summary>
    /// The tests under execution.
    /// </summary>
    public IEnumerable<Test> Tests => TestClasses.SelectMany(x => x.Tests);
}