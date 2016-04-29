using System;

namespace Fixie.Execution
{
    [Serializable]
    public abstract class CaseCompleted : Message
    {
        protected CaseCompleted(
            MethodGroup methodGroup, string name, CaseStatus status, TimeSpan duration, string output,
            CompoundException exceptions, string skipReason)
        {
            MethodGroup = methodGroup;
            Name = name;
            Status = status;
            Duration = duration;
            Output = output;
            Exceptions = exceptions;
            SkipReason = skipReason;
        }

        public MethodGroup MethodGroup { get; }
        public string Name { get; }
        public CaseStatus Status { get; }
        public TimeSpan Duration { get; }
        public string Output { get; }
        public CompoundException Exceptions { get; }
        public string SkipReason { get; }
    }
}