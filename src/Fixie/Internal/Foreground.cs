namespace Fixie.Internal;

class Foreground : IDisposable
{
    public Foreground(ConsoleColor color) => Console.ForegroundColor = color;

    public void Dispose() => Console.ResetColor();

    public static Foreground Red => new(ConsoleColor.Red);

    public static Foreground Yellow => new(ConsoleColor.Yellow);

    public static Foreground Green => new(ConsoleColor.Green);
}