using System;

namespace Fixie.Samples.Skipped
{
    [Skip]
    public class SkipClassTests
    {
        public void ShouldNotBeCalled()
        {
            throw new Exception("This test should be skipped.");
        }
    }

    [Explicit]
    public class ExplicitClassTests
    {
        public void TestWithinExplicitClass()
        {
            throw new Exception("TestWithinExplicitClass was invoked explicitly.");
        }
    }
}