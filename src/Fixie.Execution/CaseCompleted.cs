namespace Fixie.Execution
{
    using System;
    using System.Reflection;

    public abstract class CaseCompleted : Message
    {
        protected CaseCompleted(
            Type @class,
            MethodInfo method,
            CaseStatus status,
            string name,
            string output,
            TimeSpan duration,
            CompoundException exceptions,
            string skipReason)
        {
            Class = @class;
            Method = method;
            Status = status;
            Name = name;
            Output = output;
            Duration = duration;
            Exceptions = exceptions;
            SkipReason = skipReason;
        }

        public Type Class { get; }
        public MethodInfo Method { get; }
        public CaseStatus Status { get; }
        public string Name  { get; }
        public string Output  { get; }
        public TimeSpan Duration  { get; }
        public CompoundException Exceptions { get; }
        public string SkipReason { get; }
    }
}