namespace Fixie.Tests;

class RedirectedConsole : IDisposable
{
    readonly TextWriter original;
    readonly StringWriter console;

    public RedirectedConsole()
    {
        console = new StringWriter();
        original = System.Console.Out;
        System.Console.SetOut(console);
    }

    public string Output => console.ToString();

    public void Dispose()
    {
        System.Console.SetOut(original);
        console.Dispose();
    }
}