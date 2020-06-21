namespace Fixie.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Fixie.Internal;
    using Fixie.Internal.Listeners;
    using static System.Environment;

    public class StubListener :
        Handler<TestDiscovered>,
        Handler<CaseSkipped>,
        Handler<CasePassed>,
        Handler<CaseFailed>
    {
        readonly List<string> log = new List<string>();

        public void Handle(TestDiscovered message)
        {
            var test = new Test(message.Method);
            log.Add($"{test.Name} discovered");
        }

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
            log.Add($"{message.Name} failed: {message.Exception.Message}{SimplifyLiterateStackTrace(message.Exception.LiterateStackTrace())}");
        }

        static string SimplifyLiterateStackTrace(string literateStackTrace)
        {
            var stackTrace = literateStackTrace;

            stackTrace =
                string.Join(NewLine,
                    stackTrace.Split(new[] { NewLine }, StringSplitOptions.RemoveEmptyEntries)
                        .Where(x => !x.StartsWith("   at "))
                        .Where(x => x != "--- End of stack trace from previous location where exception was thrown ---"));

            return stackTrace == "" ? stackTrace : NewLine + stackTrace;
        }

        public IEnumerable<string> Entries => log;
    }
}