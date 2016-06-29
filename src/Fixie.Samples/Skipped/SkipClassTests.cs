namespace Fixie.Samples.Skipped
{
    using System;

    [Skip]
    public class SkipClassTests
    {
        public void ShouldNotBeCalled()
        {
            throw new Exception("This test should be skipped.");
        }
    }
}