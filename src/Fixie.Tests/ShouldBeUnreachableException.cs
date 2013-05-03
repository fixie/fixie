using System;

namespace Fixie.Tests
{
    public class ShouldBeUnreachableException : Exception
    {
        public ShouldBeUnreachableException()
            : base("This exception should not have been reachable.") { }
    }
}