namespace Fixie.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Fixie.Execution;
    using static System.Environment;

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
            log.Add($"{message.Name} failed: {SimplifyCompoundStackTrace(message.Exception.StackTrace)}");
        }

        static string SimplifyCompoundStackTrace(string compoundStackTrace)
        {
            var stackTrace = compoundStackTrace;

            var regexNewLine = Regex.Escape(NewLine);

            stackTrace =
                String.Join(NewLine,
                    stackTrace.Split(new[] { NewLine }, StringSplitOptions.RemoveEmptyEntries)
                        .Where(x => !x.StartsWith("   at "))
                        .Where(x => x != "--- End of stack trace from previous location where exception was thrown ---"));

            stackTrace = Regex.Replace(stackTrace,
                @"===== Secondary Exception: [a-zA-Z\.]+ =====" + regexNewLine + "([^" + regexNewLine + "]+)(" + regexNewLine + ")?",
                "    Secondary Failure: $1" + NewLine, RegexOptions.Multiline);

            stackTrace = Regex.Replace(stackTrace,
                @"------- Inner Exception: [a-zA-Z\.]+ -------" + regexNewLine + "([^" + regexNewLine + "]+)(" + regexNewLine + ")?",
                "    Inner Exception: $1" + NewLine, RegexOptions.Multiline);

            return stackTrace.Trim();
        }

        public IEnumerable<string> Entries => log;
    }
}