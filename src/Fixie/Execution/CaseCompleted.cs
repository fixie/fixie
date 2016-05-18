namespace Fixie.Execution
{
    using System;

    [Serializable]
    public abstract class CaseCompleted : Message
    {
        protected CaseCompleted(
            MethodGroup methodGroup, string name, CaseStatus status, TimeSpan duration, string output)
        {
            MethodGroup = methodGroup;
            Name = name;
            Status = status;
            Duration = duration;
            Output = output;
        }

        public MethodGroup MethodGroup { get; }
        public string Name { get; }
        public CaseStatus Status { get; }
        public TimeSpan Duration { get; }
        public string Output { get; }
    }
}