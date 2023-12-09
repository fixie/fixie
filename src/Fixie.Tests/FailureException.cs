using System;
using System.Runtime.CompilerServices;

namespace Fixie.Tests;

public class FailureException : Exception
{
    public FailureException([CallerMemberName] string member = default!)
        : base($"'{member}' failed!") { }
}