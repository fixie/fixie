using System;

namespace Fixie.Samples.Skipped
{
    [Explicit]
    public class ExplicitClassTests
    {
        public void TestWithinExplicitClass()
        {
            throw new Exception("TestWithinExplicitClass was invoked explicitly.");
        }
    }
}