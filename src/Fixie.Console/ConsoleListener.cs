using System;
using System.IO;
using Fixie.Execution;

namespace Fixie.ConsoleRunner
{
    public class ConsoleListener :
        IHandler<AssemblyStarted>,
        IHandler<SkipResult>,
        IHandler<PassResult>,
        IHandler<FailResult>,
        IHandler<AssemblyCompleted>
    {
        public void Handle(AssemblyStarted message)
        {
            Console.WriteLine($"------ Testing Assembly {Path.GetFileName(message.Assembly.Location)} ------");
            Console.WriteLine();
        }

        public void Handle(SkipResult result)
        {
            var optionalReason = result.SkipReason == null ? null : ": " + result.SkipReason;

            using (Foreground.Yellow)
                Console.WriteLine($"Test '{result.Name}' skipped{optionalReason}");
        }

        public void Handle(PassResult result)
        {
        }

        public void Handle(FailResult result)
        {
            using (Foreground.Red)
                Console.WriteLine($"Test '{result.Name}' failed: {result.Exceptions.PrimaryException.DisplayName}");
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