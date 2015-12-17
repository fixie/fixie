using System.Collections.Generic;
using System.Text;
using Fixie.Execution;

namespace Fixie.Tests
{
    public class StubListener : IHandler<CaseResult>
    {
        readonly List<string> log = new List<string>();

        public void Handle(CaseResult message)
        {
            if (message.Status == CaseStatus.Passed)
                Passed(message);
            else if (message.Status == CaseStatus.Failed)
                Failed(message);
            else if (message.Status == CaseStatus.Skipped)
                Skipped(message);
        }

        void Passed(CaseResult message)
        {
            log.Add($"{message.Name} passed.");
        }

        void Failed(CaseResult message)
        {
            var entry = new StringBuilder();

            var primaryException = message.Exceptions.PrimaryException;

            entry.Append($"{message.Name} failed: {primaryException.Message}");

            var walk = primaryException;
            while (walk.InnerException != null)
            {
                walk = walk.InnerException;
                entry.AppendLine();
                entry.Append($"    Inner Exception: {walk.Message}");
            }

            foreach (var secondaryException in message.Exceptions.SecondaryExceptions)
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

        void Skipped(CaseResult message)
        {
            var optionalReason = message.SkipReason == null ? "." : ": " + message.SkipReason;
            log.Add($"{message.Name} skipped{optionalReason}");
        }

        public IEnumerable<string> Entries => log;
    }
}