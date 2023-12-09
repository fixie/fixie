namespace Fixie.Internal;

using System;
using System.IO;

class ConsoleRedirectionBoundary : IDisposable
{
    readonly TextWriter original;

    public ConsoleRedirectionBoundary() => original = Console.Out;

    void Revert() => Console.SetOut(original);

    public void Dispose() => Revert();
}