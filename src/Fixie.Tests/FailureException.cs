using System.Runtime.CompilerServices;

namespace Fixie.Tests;

public class FailureException([CallerMemberName] string member = default!) :
    Exception($"'{member}' failed!");