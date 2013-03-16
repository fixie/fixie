using System;
using System.Linq;

namespace Fixie.Console
{
    using Console = System.Console;

    class ConsoleListener : Listener
    {
        public void CaseFailed(Case @case, Exception ex)
        {
            using (Foreground.Red)
                Console.WriteLine("{0}", @case.Name);

            using (Foreground.DarkGray)
                Console.WriteLine(Indent(ex.GetType().FullName + ":"));

            Console.WriteLine(Indent(ex.Message));
            Console.WriteLine();

            using (Foreground.DarkGray)
                Console.WriteLine(Indent("Stack Trace:"));

            Console.WriteLine(Indent(ex.StackTrace));
            Console.WriteLine();
        }

        static string Indent(string text)
        {
            var lines = NormalizeLineEndings(text).Split(new[] { "\n" }, StringSplitOptions.None);

            return String.Join(Environment.NewLine, lines.Select(x => "   " + x));
        }

        private static string NormalizeLineEndings(string input)
        {
            return input.Replace("\r\n", "\n").Replace('\r', '\n');
        }
    }
}