using System;

namespace Fixie
{
    public class CaseResult
    {
        static readonly Lazy<CaseResult> LazyPass = new Lazy<CaseResult>(() => new CaseResult(true, null));

        public static CaseResult Pass()
        {
            return LazyPass.Value;
        }

        public static CaseResult Fail(Exception exception)
        {
            return new CaseResult(false, exception);
        }

        CaseResult(bool passed, Exception exception)
        {
            Passed = passed;
            Exception = exception;
        }

        public bool Passed { get; private set; }

        public Exception Exception { get; private set; }
    }
}