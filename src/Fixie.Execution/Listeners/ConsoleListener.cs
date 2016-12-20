namespace Fixie.Execution.Listeners
{
    using System;
    using System.IO;
    using Execution;

    public class ConsoleListener : LongLivedMarshalByRefObject, Listener
    {
        public void AssemblyStarted(AssemblyInfo message)
        {
            Console.WriteLine("------ Testing Assembly {0} ------", Path.GetFileName(message.Location));
            Console.WriteLine();
        }

        public void CaseSkipped(CaseSkipped message)
        {
            using (Foreground.Yellow)
                Console.WriteLine("Test '{0}' skipped{1}", message.Name, message.SkipReason == null ? null : ": " + message.SkipReason);
        }

        public void CasePassed(CasePassed message)
        {
        }

        public void CaseFailed(CaseFailed message)
        {
            using (Foreground.Red)
                Console.WriteLine("Test '{0}' failed: {1}", message.Name, message.Exceptions.PrimaryException.DisplayName);
            Console.WriteLine(message.Exceptions.CompoundStackTrace);
            Console.WriteLine();
        }

        public void AssemblyCompleted(AssemblyInfo message, AssemblyResult result)
        {
            Console.WriteLine(result.Summary);
            Console.WriteLine();
        }
    }
}