﻿using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Fixie.Execution;

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
            log.Add($"{result.Name} skipped{(result.SkipReason == null ? "." : ": " + result.SkipReason)}");
        }

        public void CasePassed(PassResult result)
        {
            log.Add($"{result.Name} passed.");
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

        public void AssemblyCompleted(Assembly assembly, AssemblyResult result)
        {
        }

        public IEnumerable<string> Entries
        {
            get { return log; }
        }
    }
}