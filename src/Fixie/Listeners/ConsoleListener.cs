using System;
using System.Reflection;

namespace Fixie.Listeners
{
    public class ConsoleListener : Listener
    {
        public void AssemblyStarted(Assembly assembly)
        {
            Console.WriteLine("------ Testing Assembly {0} ------", assembly.FileName());
            Console.WriteLine();
        }

        public void CaseSkipped(Case @case)
        {
            using (Foreground.Yellow)
                Console.WriteLine("Test '{0}' skipped", @case.Name);
        }

        public void CasePassed(PassResult result)
        {
        }

        public void CaseFailed(FailResult result)
        {
            var @case = result.Case;

            using (Foreground.Red)
                Console.WriteLine("Test '{0}' failed: {1}", @case.Name, result.PrimaryExceptionTypeName());
            result.WriteCompoundStackTraceTo(Console.Out);
            Console.WriteLine();
            Console.WriteLine();
        }

        public void AssemblyCompleted(Assembly assembly, AssemblyResult result)
        {
            var assemblyName = typeof(ConsoleListener).Assembly.GetName();
            var name = assemblyName.Name;
            var version = assemblyName.Version;

            Console.WriteLine("{0} passed, {1} failed , {2} skipped ({3} {4}).", result.Passed, result.Failed, result.Skipped, name, version);
            Console.WriteLine();
        }
    }
}