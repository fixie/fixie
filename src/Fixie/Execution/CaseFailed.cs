namespace Fixie.Execution
{
    using System;

    public class CaseFailed : CaseCompleted
    {
        public CaseFailed(Case @case, AssertionLibraryFilter filter)
            : base(@case)
        {
            Exception = new CompoundException(@case.Exception, filter);

            Type = Exception.Type;
            Message = Exception.Message;
            FailedAssertion = Exception.FailedAssertion;
            StackTrace = Exception.StackTrace;
        }

        [Obsolete]
        public CompoundException Exception { get; }

        public string Type { get; }
        public string Message { get; }
        public bool FailedAssertion { get; }
        public string StackTrace { get; }
    }
}