using System.Collections.Generic;
using System.Text;
using Fixie.Execution;

namespace Fixie.Tests
{
    public class StubListener :
        Handler<CaseSkipped>,
        Handler<CasePassed>,
        Handler<CaseFailed>
    {
        readonly List<string> log = new List<string>();

        public void Handle(CaseSkipped message)
        {
            var optionalReason = message.SkipReason == null ? null : ": " + message.SkipReason;
            log.Add($"{message.Name} skipped{optionalReason}");
        }

        public void Handle(CasePassed message)
        {
            log.Add($"{message.Name} passed");
        }

        public void Handle(CaseFailed message)
        {
            var entry = new StringBuilder();

            var primaryException = message.Exceptions.PrimaryException;

            entry.AppendFormat("{0} failed: {1}", message.Name, primaryException.Message);

            var walk = primaryException;
            while (walk.InnerException != null)
            {
                walk = walk.InnerException;
                entry.AppendLine();
                entry.AppendFormat("    Inner Exception: {0}", walk.Message);
            }

            foreach (var secondaryException in message.Exceptions.SecondaryExceptions)
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

        public IEnumerable<string> Entries => log;
    }
}