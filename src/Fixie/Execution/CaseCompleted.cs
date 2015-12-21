using System;
using Fixie.Internal;

namespace Fixie.Execution
{
    [Serializable]
    public class CaseCompleted : IMessage
    {
        public static CaseCompleted Passed(Case @case)
        {
            return new CaseCompleted(
                methodGroup: @case.MethodGroup,
                name: @case.Name,
                status: CaseStatus.Passed,
                duration: @case.Duration,
                output: @case.Output,
                message: null,
                exceptionType: null,
                stackTrace: null,
                assertionFailed: false
                );
        }

        public static CaseCompleted Failed(Case @case, AssertionLibraryFilter filter)
        {
            var exception = new CompoundException(@case.Exceptions, filter);

            return new CaseCompleted(
                methodGroup: @case.MethodGroup,
                name: @case.Name,
                status: CaseStatus.Failed,
                duration: @case.Duration,
                output: @case.Output,
                message: exception.Message,
                exceptionType: exception.Type,
                stackTrace: exception.CompoundStackTrace,
                assertionFailed: exception.FailedAssertion
                );
        }

        public static CaseCompleted Skipped(Case @case, string reason)
        {
            return new CaseCompleted(
                methodGroup: @case.MethodGroup,
                name: @case.Name,
                status: CaseStatus.Skipped,
                duration: @case.Duration,
                output: @case.Output,
                message: reason,
                exceptionType: null,
                stackTrace: null,
                assertionFailed: false
                );
        }

        CaseCompleted(
            MethodGroup methodGroup, string name, CaseStatus status, TimeSpan duration, string output,
            string message, string exceptionType, string stackTrace, bool assertionFailed)
        {
            MethodGroup = methodGroup;
            Name = name;
            Status = status;
            Duration = duration;
            Output = output;
            Message = message;
            ExceptionType = exceptionType;
            StackTrace = stackTrace;
            AssertionFailed = assertionFailed;
        }

        public MethodGroup MethodGroup { get; }
        public string Name { get; }
        public CaseStatus Status { get; }
        public TimeSpan Duration { get; }
        public string Output { get; }
        public string Message { get; }
        public string ExceptionType { get; }
        public string StackTrace { get; }
        public bool AssertionFailed { get; }
    }
}