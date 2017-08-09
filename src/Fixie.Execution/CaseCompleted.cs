namespace Fixie.Execution
{
    using System;
    using System.Reflection;

    public abstract class CaseCompleted : Message
    {
        protected CaseCompleted(Case @case)
        {
            Class = @case.Class;
            Method = @case.Method;
            Name = @case.Name;
            Duration = @case.Duration;
            Output = @case.Output;
        }

        public Type Class { get; }
        public MethodInfo Method { get; }
        public string Name { get; }
        public TimeSpan Duration { get; }
        public string Output { get; }
    }
}