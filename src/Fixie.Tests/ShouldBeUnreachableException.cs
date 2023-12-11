using System.Runtime.CompilerServices;

namespace Fixie.Tests;

public class ShouldBeUnreachableException([CallerMemberName] string member = default!) :
    Exception($"'{member}' reached a line of code thought to be unreachable.");