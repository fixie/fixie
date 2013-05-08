using System;
using System.IO;
using System.Linq;
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

        public void CaseFailed(Case @case, Exception[] exceptions)
        {
            using (Foreground.Red)
                Console.WriteLine("Test '{0}' failed: {1}", @case.Name, exceptions.First().GetType().FullName);
            Console.Out.WriteCompoundStackTrace(exceptions);
            Console.WriteLine();
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