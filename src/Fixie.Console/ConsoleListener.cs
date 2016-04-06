using System;
using System.IO;
using Fixie.Execution;

namespace Fixie.ConsoleRunner
{
    public class ConsoleListener : Listener
    {
        public void Handle(AssemblyInfo assembly)
        {
            Console.WriteLine("------ Testing Assembly {0} ------", Path.GetFileName(assembly.Location));
            Console.WriteLine();
        }

        public void Handle(SkipResult result)
        {
            using (Foreground.Yellow)
                Console.WriteLine("Test '{0}' skipped{1}", result.Name, result.SkipReason == null ? null : ": " + result.SkipReason);
        }

        public void Handle(PassResult result)
        {
        }

        public void Handle(FailResult result)
        {
            using (Foreground.Red)
                Console.WriteLine("Test '{0}' failed: {1}", result.Name, result.Exceptions.PrimaryException.DisplayName);
            Console.WriteLine(result.Exceptions.CompoundStackTrace);
            Console.WriteLine();
        }

        public void Handle(AssemblyCompleted message)
        {
            Console.WriteLine(message.Result.Summary);
            Console.WriteLine();
        }
    }
}