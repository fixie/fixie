using System;
using System.Collections.Generic;
using System.Text;
using Fixie.Execution;

namespace Fixie.Tests
{
    public class StubListener : Listener
    {
        readonly List<string> log = new List<string>();

        public void AssemblyStarted(AssemblyInfo assembly)
        {
        }

        public void CaseSkipped(SkipResult result)
        {
            log.Add(string.Format("{0} skipped{1}", result.Name, result.SkipReason == null ? "." : ": " + result.SkipReason));
        }

        public void CasePassed(PassResult result)
        {
            log.Add(string.Format("{0} passed.", result.Name));
        }

        public void CaseFailed(FailResult result)
        {
            LogExceptionalResult(result.Exceptions, result.Name, "failed");
        }

        public void CaseInconclusive(InconclusiveResult result)
        {
            throw new NotImplementedException();
        }

        void LogExceptionalResult(CompoundException exceptions, string name, string status)
        {
            var entry = new StringBuilder();

            var primaryException = exceptions.PrimaryException;

            entry.AppendFormat("{0} {1}: {2}", name, status, primaryException.Message);

            var walk = primaryException;
            while (walk.InnerException != null)
            {
                walk = walk.InnerException;
                entry.AppendLine();
                entry.AppendFormat("    Inner Exception: {0}", walk.Message);
            }

            foreach (var secondaryException in exceptions.SecondaryExceptions)
            {
                entry.AppendLine();
                entry.AppendFormat("    Secondary Failure: {0}", secondaryException.Message);

                walk = secondaryException;
                while (walk.InnerException != null)
                {
                    walk = walk.InnerException;
                    entry.AppendLine();
                    entry.AppendFormat("        Inner Exception: {0}", walk.Message);
                }
            }

            log.Add(entry.ToString());
        }

        public void AssemblyCompleted(AssemblyInfo assembly, AssemblyResult result)
        {
        }

        public IEnumerable<string> Entries
        {
            get { return log; }
        }
    }
}