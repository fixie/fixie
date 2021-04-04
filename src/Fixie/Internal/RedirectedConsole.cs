namespace Fixie.Internal
{
    using System;
    using System.IO;

    class RedirectedConsole : IDisposable
    {
        readonly TextWriter original;
        readonly StringWriter console;

        public RedirectedConsole()
        {
            console = new StringWriter();
            original = Console.Out;
            Console.SetOut(console);
        }

        public string Output => console.ToString();

        public void Dispose()
        {
            Console.SetOut(original);
            console.Dispose();
        }
    }
}