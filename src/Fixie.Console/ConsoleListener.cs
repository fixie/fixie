using System;
using System.IO;
using Fixie.Execution;

namespace Fixie.ConsoleRunner
{
    public class ConsoleListener : Listener
    {
        public void Handle(AssemblyInfo message)
        {
            Console.WriteLine("------ Testing Assembly {0} ------", Path.GetFileName(message.Location));
            Console.WriteLine();
        }

        public void Handle(SkipResult message)
        {
            using (Foreground.Yellow)
                Console.WriteLine("Test '{0}' skipped{1}", message.Name, message.SkipReason == null ? null : ": " + message.SkipReason);
        }

        public void Handle(PassResult message)
        {
        }

        public void Handle(FailResult message)
        {
            using (Foreground.Red)
                Console.WriteLine("Test '{0}' failed: {1}", message.Name, message.Exceptions.PrimaryException.DisplayName);
            Console.WriteLine(message.Exceptions.CompoundStackTrace);
            Console.WriteLine();
        }

        public void Handle(AssemblyCompleted message)
        {
            Console.WriteLine(message.Result.Summary);
            Console.WriteLine();
        }
    }
}