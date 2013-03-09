using System;

namespace Fixie.Console
{
    public class ConsoleListener : Listener
    {
        public void CaseFailed(Case @case, Exception ex)
        {
            Line("{0} threw {1}:", @case.Name, ex.GetType().FullName);
            Line(ex.Message);
            Line(ex.StackTrace);
            Line();
        }

        static void Line()
        {
            System.Console.WriteLine();
        }

        static void Line(string format, params object[] args)
        {
            System.Console.WriteLine(format, args);
        }
    }
}