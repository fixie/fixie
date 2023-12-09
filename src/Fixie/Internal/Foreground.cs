namespace Fixie.Internal;

class Foreground : IDisposable
{
    readonly ConsoleColor before;

    public Foreground(ConsoleColor color)
    {
        before = Console.ForegroundColor;
        Console.ForegroundColor = color;
    }

    public void Dispose() => Console.ForegroundColor = before;

    public static Foreground Red => new(ConsoleColor.Red);

    public static Foreground Yellow => new(ConsoleColor.Yellow);

    public static Foreground Green => new(ConsoleColor.Green);
}