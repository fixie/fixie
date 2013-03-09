using System;

namespace Fixie.Console
{
    public class ConsoleListener : Listener
    {
        public void CaseFailed(Case @case, Exception ex)
        {
            System.Console.WriteLine("{0} failed: {1}", @case.Name, ex.Message);
            System.Console.WriteLine(ex.StackTrace);
        }
    }
}