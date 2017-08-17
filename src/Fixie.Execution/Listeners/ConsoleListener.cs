﻿namespace Fixie.Execution.Listeners
{
    using System;
    using System.IO;
    using Execution;

    public class ConsoleListener :
        Handler<AssemblyStarted>,
        Handler<CaseSkipped>,
        Handler<CaseFailed>,
        Handler<AssemblyCompleted>
    {
        public void Handle(AssemblyStarted message)
        {
            Console.WriteLine($"------ Testing Assembly {Path.GetFileName(message.Assembly.Location)} ------");
            Console.WriteLine();
        }

        public void Handle(CaseSkipped message)
        {
            var optionalReason = message.Reason == null ? null : ": " + message.Reason;

            using (Foreground.Yellow)
                Console.WriteLine($"Test '{message.Name}' skipped{optionalReason}");
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
            Console.WriteLine($"{message.Summary} ({Framework.Version}).");
            Console.WriteLine();
        }
    }
}