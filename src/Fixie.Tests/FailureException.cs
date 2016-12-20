namespace Fixie.Tests
{
    using System;
    using System.Runtime.CompilerServices;

    public class FailureException : Exception
    {
        public FailureException([CallerMemberName] string member = null)
            : base($"'{member}' failed!") { }
    }
}