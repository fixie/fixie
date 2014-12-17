using System;
using System.IO;

namespace Fixie.VisualStudio.TestAdapter
{
    /// <summary>
    /// Allows the Visual Studio test adapter to redirect the console that is owned by the test assembly AppDomain.
    /// </summary>
    public class ConsoleRedirectionProxy : MarshalByRefObject
    {
        public void RedirectConsole(TextWriter console)
        {
            Console.SetOut(console);
        }
    }
}