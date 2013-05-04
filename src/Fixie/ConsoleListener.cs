using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

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
                Console.WriteLine("Test '{0}' failed: {1}", @case.Name, exceptions.First().Message);
            GetCompoundStackTrace(exceptions);
            Console.WriteLine();
            Console.WriteLine();
        }

        static void GetCompoundStackTrace(IEnumerable<Exception> exceptions)
        {
            bool isPrimaryException = true;

            foreach (var ex in exceptions)
            {
                if (isPrimaryException)
                {
                    Console.Write(ex.StackTrace);
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine();
                    Console.WriteLine("===== Secondary Exception =====");
                    using (Foreground.DarkGray)
                        Console.WriteLine(ex.GetType().FullName);
                    Console.WriteLine(ex.Message);
                    Console.Write(ex.StackTrace);
                }

                var walk = ex;
                while (walk.InnerException != null)
                {
                    walk = walk.InnerException;
                    Console.WriteLine();
                    Console.WriteLine();
                    using (Foreground.DarkGray)
                        Console.WriteLine("----- Inner Exception -----");

                    Console.WriteLine(walk.GetType().FullName);
                    Console.WriteLine(walk.Message);
                    Console.Write(walk.StackTrace);
                }

                isPrimaryException = false;
            }
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