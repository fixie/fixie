using System;
using System.Text;

namespace Fixie.Tests
{
    public class StubListener : Listener
    {
        readonly StringBuilder log = new StringBuilder();

        public void CaseFailed(Case @case, Exception ex)
        {
            log.AppendFormat("{0} failed: {1}", @case.Name, ex.Message);
        }

        public override string ToString()
        {
            return log.ToString();
        }
    }
}