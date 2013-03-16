using System;

namespace Fixie.Console
{
    public class ConsoleListener : Listener
    {
        public void CaseFailed(Case @case, Exception ex)
        {
            System.Console.WriteLine("{0} threw {1}:", @case.Name, ex.GetType().FullName);
            System.Console.WriteLine(ex.Message);
            System.Console.WriteLine(ex.StackTrace);
            System.Console.WriteLine();
        }
    }
}