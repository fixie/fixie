namespace Fixie.Reports
{
    using System;
    using Internal;

    public class TestPassed : TestCompleted
    {
        internal TestPassed(Case @case, TimeSpan duration, string output)
            : base(@case, duration, output)
        {
        }
    }
}