namespace Fixie.Reports
{
    using System;

    public class TestPassed : TestCompleted
    {
        internal TestPassed(string test, string name, TimeSpan duration, string output)
            : base(test, name, duration, output)
        {
        }
    }
}