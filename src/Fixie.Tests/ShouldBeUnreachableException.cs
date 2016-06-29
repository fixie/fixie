namespace Fixie.Tests
{
    using System;
    using System.Runtime.CompilerServices;

    public class ShouldBeUnreachableException : Exception
    {
        public ShouldBeUnreachableException([CallerMemberName] string member = null)
            : base($"'{member}' reached a line of code thought to be unreachable.") { }
    }
}