namespace Fixie.Tests
{
    using System.Collections.Generic;
    using System.Text;
    using Fixie.Execution;

    public class StubListener :
        Handler<CaseSkipped>,
        Handler<CasePassed>,
        Handler<CaseFailed>
    {
        readonly List<string> log = new List<string>();

        public void Handle(CaseSkipped message)
        {
            var optionalReason = message.Reason == null ? null : ": " + message.Reason;
            log.Add($"{message.Name} skipped{optionalReason}");
        }

        public void Handle(CasePassed message)
        {
            log.Add($"{message.Name} passed");
        }

        public void Handle(CaseFailed message)
        {
            var entry = new StringBuilder();

            var primaryException = message.Exception.PrimaryException;

            entry.Append($"{message.Name} failed: {message.Exception.Message}");

            var walk = primaryException;
            while (walk.InnerException != null)
            {
                walk = walk.InnerException;
                entry.AppendLine();
                entry.Append($"    Inner Exception: {walk.Message}");
            }

            foreach (var secondaryException in message.Exception.SecondaryExceptions)
            {
                entry.AppendLine();
                entry.Append($"    Secondary Failure: {secondaryException.Message}");

                walk = secondaryException;
                while (walk.InnerException != null)
                {
                    walk = walk.InnerException;
                    entry.AppendLine();
                    entry.Append($"        Inner Exception: {walk.Message}");
                }
            }

            log.Add(entry.ToString());
        }

        public IEnumerable<string> Entries
        {
            get { return log; }
        }
    }
}