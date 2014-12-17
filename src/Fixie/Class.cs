using System;
using System.Collections.Generic;
using Fixie.Internal;

namespace Fixie
{
    /// <summary>
    /// A test class being executed.
    /// </summary>
    public class Class : BehaviorContext
    {
        public Class(Type type, IReadOnlyList<Case> cases)
        {
            Type = type;
            Cases = cases;
        }

        /// <summary>
        /// Gets the test class type being executed.
        /// </summary>
        public Type Type { get; private set; }

        /// <summary>
        /// Gets the test cases being executed for this test class.
        /// </summary>
        public IReadOnlyList<Case> Cases { get; private set; }

        /// <summary>
        /// Includes the given exception in all of the test class's
        /// executing test cases, indicating that they have all failed.
        /// </summary>
        public void Fail(Exception reason)
        {
            foreach (var @case in Cases)
                @case.Fail(reason);
        }
    }
}