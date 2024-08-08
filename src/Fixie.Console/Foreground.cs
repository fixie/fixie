namespace Fixie.Console;

class Foreground : IDisposable
{
    public Foreground(ConsoleColor color) => System.Console.ForegroundColor = color;

    public void Dispose() => System.Console.ResetColor();

    public static Foreground Red => new(ConsoleColor.Red);

    public static Foreground Yellow => new(ConsoleColor.Yellow);

    public static Foreground Green => new(ConsoleColor.Green);
}