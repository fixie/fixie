using System;

namespace Fixie.Results
{
    [Serializable]
    public class CaseResult
    {
        public static CaseResult Passed(string name, TimeSpan duration)
        {
            return new CaseResult(name, CaseStatus.Passed, duration, null, null);
        }

        public static CaseResult Failed(string name, TimeSpan duration, string message, string stackTrace)
        {
            return new CaseResult(name, CaseStatus.Failed, duration, message, stackTrace);
        }

        public static CaseResult Skipped(string name)
        {
            return new CaseResult(name, CaseStatus.Skipped, TimeSpan.Zero, null, null);
        }

        CaseResult(string name, CaseStatus status, TimeSpan duration, string message, string stackTrace)
        {
            Name = name;
            Status = status;
            Duration = duration;
            Message = message;
            StackTrace = stackTrace;
        }

        public string Name { get; private set; }
        public CaseStatus Status { get; private set; }
        public TimeSpan Duration { get; private set; }
        public string Message { get; private set; }
        public string StackTrace { get; private set; }
    }
}