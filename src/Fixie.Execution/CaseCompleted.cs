namespace Fixie.Execution
{
    using System;
    using System.Reflection;

    public abstract class CaseCompleted : Message
    {
        protected CaseCompleted(
            Type @class, MethodInfo method, string name, CaseStatus status, TimeSpan duration, string output)
        {
            Class = @class;
            Method = method;
            Name = name;
            Status = status;
            Duration = duration;
            Output = output;
        }

        public Type Class { get; }
        public MethodInfo Method { get; }
        public string Name { get; }
        public CaseStatus Status { get; }
        public TimeSpan Duration { get; }
        public string Output { get; }
    }
}