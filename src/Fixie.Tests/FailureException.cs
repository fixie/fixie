using System;
using System.Runtime.CompilerServices;

namespace Fixie.Tests
{
    public class FailureException : Exception
    {
        public FailureException([CallerMemberName] string member = null)
            : base("'" + member + "' failed!") { }
    }
}