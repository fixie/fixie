namespace Fixie.Execution.Listeners
{
    using System;
    using Execution;

    public class ConsoleListener :
        Handler<CaseSkipped>,
        Handler<CaseFailed>,
        Handler<AssemblyCompleted>
    {
        public void Handle(CaseSkipped message)
        {
            var hasReason = message.Reason != null;

            using (Foreground.Yellow)
                Console.WriteLine($"Test '{message.Name}' skipped{(hasReason ? ":" : null)}");

            if (hasReason)
                Console.WriteLine($"{message.Reason}");

            Console.WriteLine();
        }

        public void Handle(CaseFailed message)
        {
            using (Foreground.Red)
                Console.WriteLine($"Test '{message.Name}' failed:{(message.Exception.FailedAssertion ? "" : " " + message.Exception.Type)}");
            Console.WriteLine(message.Exception.Message);
            Console.WriteLine(message.Exception.StackTrace);
            Console.WriteLine();
        }

        public void Handle(AssemblyCompleted message)
        {
            Console.WriteLine($"{message.Summary}");
            Console.WriteLine();
        }
    }
}