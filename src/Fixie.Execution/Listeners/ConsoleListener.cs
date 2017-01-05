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
            Console.WriteLine("------ Testing Assembly {0} ------", Path.GetFileName(message.Assembly.Location));
            Console.WriteLine();
        }

        public void Handle(CaseSkipped message)
        {
            summary.Add(message);

            using (Foreground.Yellow)
                Console.WriteLine("Test '{0}' skipped{1}", message.Name, message.Reason == null ? null : ": " + message.Reason);
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
            Console.WriteLine(message.Exception.StackTrace);
            Console.WriteLine();
        }

        public void Handle(AssemblyCompleted message)
        {
            Console.WriteLine(summary);
            Console.WriteLine();
        }
    }
}