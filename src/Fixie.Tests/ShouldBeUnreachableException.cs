using System.Runtime.CompilerServices;

namespace Fixie.Tests;

public class ShouldBeUnreachableException : Exception
{
    public ShouldBeUnreachableException([CallerMemberName] string member = default!)
        : base($"'{member}' reached a line of code thought to be unreachable.") { }
}