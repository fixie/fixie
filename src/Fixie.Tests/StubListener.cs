using System;
using System.Collections.Generic;

namespace Fixie.Tests
{
    public class StubListener : Listener
    {
        readonly List<string> log = new List<string>();
        readonly RunState runState = new RunState();

        public void CasePassed(Case @case)
        {
            runState.CasePassed();
            log.Add(string.Format("{0} passed.", @case.Name));
        }

        public void CaseFailed(Case @case, Exception ex)
        {
            runState.CaseFailed();
            log.Add(string.Format("{0} failed: {1}", @case.Name, ex.Message));
        }

        public void AssemblyComplete()
        {
        }

        public RunState State { get { return runState; } }

        public IEnumerable<string> Entries
        {
            get { return log; }
        }
    }
}