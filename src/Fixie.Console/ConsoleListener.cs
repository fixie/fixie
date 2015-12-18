using System;
using System.IO;
using Fixie.Execution;

namespace Fixie.ConsoleRunner
{
    public class ConsoleListener :
        IHandler<AssemblyStarted>,
        IHandler<CaseResult>,
        IHandler<AssemblyCompleted>
    {
        public void Handle(AssemblyStarted message)
        {
            Console.WriteLine($"------ Testing Assembly {Path.GetFileName(message.Assembly.Location)} ------");
            Console.WriteLine();
        }

        public void Handle(CaseResult message)
        {
            if (message.Status == CaseStatus.Failed)
            {
                using (Foreground.Red)
                    Console.WriteLine($"Test '{message.Name}' failed: {(message.AssertionFailed ? "" : message.ExceptionType)}");
                Console.WriteLine(message.StackTrace);
                Console.WriteLine();
            }
            else if (message.Status == CaseStatus.Skipped)
            {
                var optionalReason = message.Message == null ? null : ": " + message.Message;

                using (Foreground.Yellow)
                    Console.WriteLine($"Test '{message.Name}' skipped{optionalReason}");
            }
        }

        public void Handle(AssemblyCompleted message)
        {
            Console.WriteLine(message.Result.Summary);
            Console.WriteLine();
        }
    }
}