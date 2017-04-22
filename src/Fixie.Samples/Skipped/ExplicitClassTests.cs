namespace Fixie.Samples.Skipped
{
    using System;

    [Explicit]
    public class ExplicitClassTests
    {
        public void TestWithinExplicitClass()
        {
            throw new Exception("TestWithinExplicitClass was invoked explicitly.");
        }
    }
}