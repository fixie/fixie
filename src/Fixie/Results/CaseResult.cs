using System;

namespace Fixie.Results
{
    [Serializable]
    public class CaseResult
    {
        public static CaseResult Passed(string name, TimeSpan duration)
        {
            return new CaseResult(name, CaseStatus.Passed, duration);
        }

        public static CaseResult Failed(string name, TimeSpan duration, string message, string stackTrace, string exceptionType)
        {
            return new CaseResult(name, CaseStatus.Failed, duration)
                   {
                       Message = message,
                       StackTrace = stackTrace,
                       ExceptionType = exceptionType
                   };
        }

        public static CaseResult Skipped(string name, string skipReason)
        {
            return new CaseResult(name, CaseStatus.Skipped, TimeSpan.Zero)
                   {
                       SkipReason = skipReason
                   };
        }

        CaseResult(string name, CaseStatus status, TimeSpan duration)
        {
            Name = name;
            Status = status;
            Duration = duration;
        }

        public string Name { get; private set; }
        public CaseStatus Status { get; private set; }
        public TimeSpan Duration { get; private set; }
        public string Message { get; private set; }
        public string StackTrace { get; private set; }
        public string ExceptionType { get; private set; }
        public string SkipReason { get; private set; }
    }
}