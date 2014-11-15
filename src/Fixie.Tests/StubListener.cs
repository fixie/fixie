using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Text;
using Fixie.Execution;
using Fixie.Results;

namespace Fixie.Tests
{
    public class StubListener : Listener
    {
        readonly List<string> log = new List<string>();

        public void AssemblyStarted(string assemblyFileName)
        {
        }

        public void CaseSkipped(SkipResult result)
        {
            log.Add(string.Format("{0} skipped{1}", result.Name, result.Reason == null ? "." : ": " + result.Reason));
        }

        public void CasePassed(PassResult result)
        {
            log.Add(string.Format("{0} passed.", result.Name));
        }

        public void CaseFailed(FailResult result)
        {
            var entry = new StringBuilder();

            var primaryException = result.Exceptions.PrimaryException;

            entry.AppendFormat("{0} failed: {1}", result.Name, primaryException.Message);

            var walk = primaryException;
            while (walk.InnerException != null)
            {
                walk = walk.InnerException;
                entry.AppendLine();
                entry.AppendFormat("    Inner Exception: {0}", walk.Message);
            }

            foreach (var secondaryException in result.Exceptions.SecondaryExceptions)
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

        public void AssemblyCompleted(string assemblyFileName, AssemblyResult result)
        {
        }

        public IEnumerable<string> Entries
        {
            get { return log; }
        }
    }
}