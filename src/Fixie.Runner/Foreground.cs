namespace Fixie.Runner
{
    using System;

    public class Foreground : IDisposable
    {
        readonly ConsoleColor before;

        public Foreground(ConsoleColor color)
        {
            before = Console.ForegroundColor;
            Console.ForegroundColor = color;
        }

        public void Dispose() => Console.ForegroundColor = before;

        public static Foreground Red => new Foreground(ConsoleColor.Red);
    }
}