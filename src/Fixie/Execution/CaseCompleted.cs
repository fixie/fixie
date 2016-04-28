using System;

namespace Fixie.Execution
{
    [Serializable]
    public abstract class CaseCompleted : Message
    {
        public MethodGroup MethodGroup  { get; protected set; }
        public string Name  { get; protected set; }
        public CaseStatus Status { get; protected set; }
        public TimeSpan Duration  { get; protected set; }
        public string Output  { get; protected set; }
        public CompoundException Exceptions { get; protected set; }
        public string SkipReason { get; protected set; }
    }
}