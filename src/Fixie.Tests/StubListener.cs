using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
            log.Add($"{message.Name} passed");
        }

        void Failed(CaseResult message)
        {
            log.Add($"{message.Name} failed: {String.Join(Environment.NewLine, SimplifyCompoundStackTrace(message.Message + Environment.NewLine + message.StackTrace))}");
        }

        void Skipped(CaseResult message)
        {
            var optionalReason = message.Message == null ? null : ": " + message.Message;
            log.Add($"{message.Name} skipped{optionalReason}");
        }

        static IEnumerable<string> SimplifyCompoundStackTrace(string stackTrace)
        {
            var lines = new Queue<string>(stackTrace.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));

            bool isWithinSecondaryException = false;

            while (lines.Any())
            {
                var line = lines.Dequeue();

                var prefix = "";

                if (Regex.IsMatch(line, @"===== Secondary Exception: [a-zA-Z\.]+ ====="))
                {
                    isWithinSecondaryException = true;
                    prefix = "    Secondary Failure: ";
                    line = lines.Dequeue();
                }
                else if (Regex.IsMatch(line, @"------- Inner Exception: [a-zA-Z\.]+ -------"))
                {
                    prefix = "    Inner Exception: ";

                    if (isWithinSecondaryException)
                        prefix = "    " + prefix;

                    line = lines.Dequeue();
                }

                yield return prefix + line;

                while (lines.Any() && (lines.Peek().StartsWith("   at ") || lines.Peek()== "--- End of stack trace from previous location where exception was thrown ---"))
                    lines.Dequeue();
            }
        }

        public IEnumerable<string> Entries => log;
    }
}