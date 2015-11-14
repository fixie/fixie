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
}