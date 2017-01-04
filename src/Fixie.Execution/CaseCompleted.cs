namespace Fixie.Execution
{
    using System;

    public abstract class CaseCompleted : Message
    {
        protected CaseCompleted(
            CaseStatus status,
            string name,
            MethodGroup methodGroup,
            string output,
            TimeSpan duration,
            CompoundException exceptions,
            string skipReason)
        {
            Status = status;
            Name = name;
            MethodGroup = methodGroup;
            Output = output;
            Duration = duration;
            Exceptions = exceptions;
            SkipReason = skipReason;
        }

        public CaseStatus Status { get; }
        public string Name  { get; }
        public MethodGroup MethodGroup  { get; }
        public string Output  { get; }
        public TimeSpan Duration  { get; }
        public CompoundException Exceptions { get; }
        public string SkipReason { get; }
    }
}