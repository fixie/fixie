using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Fixie.Results;

namespace Fixie.Tests
{
    public class StubListener : Listener
    {
        readonly List<string> log = new List<string>();

        public void AssemblyStarted(Assembly assembly)
        {
        }

        public void CaseSkipped(Case @case)
        {
            log.Add(string.Format("{0} skipped.", @case.Name));
        }

        public void CasePassed(PassResult result)
        {
            var @case = result.Case;
            log.Add(string.Format("{0} passed.", @case.Name));
        }

        public void CaseFailed(FailResult result)
        {
            var @case = result.Case;
            var exceptions = result.Exceptions;

            var entry = new StringBuilder();

            var primary = exceptions.First();
            entry.Append(string.Format("{0} failed: {1}", @case.Name, primary.Message));

            foreach (var exception in exceptions.Skip(1))
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