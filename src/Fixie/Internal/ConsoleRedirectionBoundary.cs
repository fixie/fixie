namespace Fixie.Internal;

class ConsoleRedirectionBoundary : IDisposable
{
    readonly TextWriter original;

    public ConsoleRedirectionBoundary() => original = Console.Out;

    void Revert() => Console.SetOut(original);

    public void Dispose() => Revert();
}