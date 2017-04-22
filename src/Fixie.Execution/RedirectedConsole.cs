namespace Fixie.Execution
{
    using System;
    using System.IO;

    public class RedirectedConsole : IDisposable
    {
        readonly TextWriter outBefore;
        readonly TextWriter errBefore;
        readonly StringWriter console;

        public RedirectedConsole()
        {
            console = new StringWriter();
            outBefore = Console.Out;
            errBefore = Console.Error;
            Console.SetOut(console);
            Console.SetError(console);
        }

        public string Output => console.ToString();

        public void Dispose()
        {
            Console.SetOut(outBefore);
            Console.SetError(errBefore);
            console.Dispose();
        }
    }
}