using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Fixie.Tests
{
    public class StubListener : Listener
    {
        readonly List<string> log = new List<string>();

        public void AssemblyStarted(Assembly assembly)
        {
        }

        public void CasePassed(Case @case)
        {
            log.Add(string.Format("{0} passed.", @case.Name));
        }

        public void CaseFailed(Case @case, Exception[] exceptions)
        {
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

        public void AssemblyCompleted(Assembly assembly, Result result)
        {
        }

        public IEnumerable<string> Entries
        {
            get { return log; }
        }
    }
}