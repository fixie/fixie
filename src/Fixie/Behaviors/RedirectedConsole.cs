using System;
using System.IO;

namespace Fixie.Behaviors
{
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

        public string Output
        {
            get { return console.ToString(); }
        }

        public void Dispose()
        {
            Console.SetOut(outBefore);
            Console.SetError(errBefore);
            console.Dispose();
        }
    }
}