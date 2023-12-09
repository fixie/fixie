namespace Fixie.Console;

class Foreground : IDisposable
{
    readonly ConsoleColor before;

    public Foreground(ConsoleColor color)
    {
        before = System.Console.ForegroundColor;
        System.Console.ForegroundColor = color;
    }

    public void Dispose() => System.Console.ForegroundColor = before;

    public static Foreground Red => new(ConsoleColor.Red);

    public static Foreground Yellow => new(ConsoleColor.Yellow);

    public static Foreground Green => new(ConsoleColor.Green);
}