using System;
using System.Collections.Generic;
using Fixie.Internal;

namespace Fixie
{
    /// <summary>
    /// A test fixture (test class instance) being executed.
    /// </summary>
    public class Fixture : BehaviorContext
    {
        public Fixture(Class testClass, object instance, IReadOnlyList<Case> cases)
        {
            Class = testClass;
            Instance = instance;
            Cases = cases;
        }

        /// <summary>
        /// Gets the test class being executed.
        /// </summary>
        public Class Class { get; }

        /// <summary>
        /// Gets the instance of the test class being executed.
        /// </summary>
        public object Instance { get; }

        /// <summary>
        /// Gets the test cases being executed for this instance of the test class.
        /// </summary>
        public IReadOnlyList<Case> Cases { get; }

        /// <summary>
        /// Includes the given exception in all of the test class instance's
        /// executing test cases, indicating that they have all failed.
        /// </summary>
        public void Fail(Exception reason)
        {
            foreach (var @case in Cases)
                @case.Fail(reason);
        }
    }
}