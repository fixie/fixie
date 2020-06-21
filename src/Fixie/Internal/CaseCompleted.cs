namespace Fixie.Internal
{
    using System;

    public abstract class CaseCompleted : Message
    {
        protected CaseCompleted(Case @case)
        {
            Test = new Test(@case.Method);
            Name = @case.Name;
            Duration = @case.Duration;
            Output = @case.Output;
        }

        public Test Test { get; }
        public string Name { get; }
        public TimeSpan Duration { get; }
        public string Output { get; }
    }
}