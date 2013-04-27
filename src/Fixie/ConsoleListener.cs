using System;
using System.IO;
using System.Reflection;

namespace Fixie
{
    public class ConsoleListener : Listener
    {
        public void RunStarted(Assembly context)
        {
            Console.WriteLine("------ Testing Assembly {0} ------", Path.GetFileName(context.Location));
            Console.WriteLine();
        }

        public void CasePassed(Case @case)
        {
        }

        public void CaseFailed(Case @case, Exception ex)
        {
            using (Foreground.Red)
                Console.WriteLine("{0}", @case.Name);

            using (Foreground.DarkGray)
                Console.WriteLine(ex.GetType().FullName);

            Console.WriteLine(ex.Message);

            using (Foreground.DarkGray)
                Console.WriteLine("Stack Trace:");

            Console.WriteLine(ex.StackTrace);
            Console.WriteLine();
        }

        public void RunComplete(Result result)
        {
            var assemblyName = typeof(ConsoleListener).Assembly.GetName();
            var name = assemblyName.Name;
            var version = assemblyName.Version;

            Console.WriteLine("{0} passed, {1} failed ({2} {3}).", result.Passed, result.Failed, name, version);
        }
    }
}