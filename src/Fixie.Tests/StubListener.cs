using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Fixie.Execution;
using Fixie.Results;

namespace Fixie.Tests
{
    public class StubListener : Listener
    {
        readonly List<string> log = new List<string>();

        public void AssemblyStarted(Assembly assembly)
        {
        }

        public void CaseSkipped(SkipResult result)
        {
            log.Add(string.Format("{0} skipped{1}", result.Case.Name, result.Reason == null ? "." : ": " + result.Reason));
        }

        public void CasePassed(PassResult result)
        {
            var @case = result.Case;
            log.Add(string.Format("{0} passed.", @case.Name));
        }

        public void CaseFailed(FailResult result)
        {
            var @case = result.Case;

            var entry = new StringBuilder();

            entry.Append(string.Format("{0} failed: {1}", @case.Name, result.Exceptions.PrimaryException.Message));

            foreach (var exception in result.Exceptions.SecondaryExceptions)
            {
                entry.AppendLine();
                entry.Append(string.Format("    Secondary Failure: {0}", exception.Message));
            }

            log.Add(entry.ToString());
        }

        public void AssemblyCompleted(Assembly assembly, AssemblyResult result)
        {
        }

        public IEnumerable<string> Entries
        {
            get { return log; }
        }
    }
}