namespace Fixie.Execution.Listeners
{
    using System;
    using System.IO;
    using Execution;

    public class ConsoleListener :
        Handler<AssemblyStarted>,
        Handler<CaseSkipped>,
        Handler<CasePassed>,
        Handler<CaseFailed>,
        Handler<AssemblyCompleted>
    {
        ExecutionSummary summary;

        public void Handle(AssemblyStarted message)
        {
            summary = new ExecutionSummary();
            Console.WriteLine($"------ Testing Assembly {Path.GetFileName(message.Assembly.Location)} ------");
            Console.WriteLine();
        }

        public void Handle(CaseSkipped message)
        {
            summary.Add(message);

            var optionalReason = message.Reason == null ? null : ": " + message.Reason;

            using (Foreground.Yellow)
                Console.WriteLine($"Test '{message.Name}' skipped{optionalReason}");
        }

        public void Handle(CasePassed message)
        {
            summary.Add(message);
        }

        public void Handle(CaseFailed message)
        {
            summary.Add(message);

            using (Foreground.Red)
                Console.WriteLine($"Test '{message.Name}' failed:{(message.Exception.FailedAssertion ? "" : " " + message.Exception.Type)}");
            Console.WriteLine(message.Exception.Message);
            Console.WriteLine(message.Exception.StackTrace);
            Console.WriteLine();
        }

        public void Handle(AssemblyCompleted message)
        {
            Console.WriteLine($"{summary} ({Framework.Version}).");
            Console.WriteLine();
            summary = null;
        }
    }
}