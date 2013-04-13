using System;
using System.Linq;

namespace Fixie
{
    public class ConsoleListener : Listener
    {
        readonly RunState runState = new RunState();

        public void CasePassed(Case @case)
        {
            runState.CasePassed();
        }

        public void CaseFailed(Case @case, Exception ex)
        {
            runState.CaseFailed();

            using (Foreground.Red)
                Console.WriteLine("{0}", @case.Name);

            using (Foreground.DarkGray)
                Console.WriteLine(Indent(ex.GetType().FullName + ":"));

            Console.WriteLine(Indent(Indent(ex.Message)));
            Console.WriteLine();

            using (Foreground.DarkGray)
                Console.WriteLine(Indent("Stack Trace:"));

            Console.WriteLine(Indent(ex.StackTrace));
            Console.WriteLine();
        }

        public void RunComplete()
        {
            var result = runState.ToResult();
            Console.WriteLine("{0} total, {1} failed", result.Total, result.Failed);
        }

        public RunState State
        {
            get { return runState; }
        }

        static string Indent(string text)
        {
            var lines = NormalizeLineEndings(text).Split(new[] { "\n" }, StringSplitOptions.None);

            return String.Join(Environment.NewLine, lines.Select(x => "   " + x));
        }

        static string NormalizeLineEndings(string input)
        {
            return input.Replace("\r\n", "\n").Replace('\r', '\n');
        }
    }
}