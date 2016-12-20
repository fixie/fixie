namespace Fixie.Tests
{
    using System.Collections.Generic;
    using System.Text;
    using Fixie.Execution;

    public class StubListener : Listener
    {
        readonly List<string> log = new List<string>();

        public void AssemblyStarted(AssemblyInfo assembly)
        {
        }

        public void CaseSkipped(SkipResult result)
        {
            var optionalReason = result.SkipReason == null ? null : ": " + result.SkipReason;
            log.Add($"{result.Name} skipped{optionalReason}");
        }

        public void CasePassed(PassResult result)
        {
            log.Add($"{result.Name} passed");
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

        public void AssemblyCompleted(AssemblyInfo assembly, AssemblyResult result)
        {
        }

        public IEnumerable<string> Entries
        {
            get { return log; }
        }
    }
}