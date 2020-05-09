namespace Fixie.Tests
{
    using System;
    using System.Runtime.CompilerServices;

    public class FailureException : Exception
    {
        public FailureException([CallerMemberName] string member = default!)
            : base($"'{member}' failed!") { }
    }
}