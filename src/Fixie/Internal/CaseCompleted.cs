namespace Fixie.Internal
{
    using System;

    public abstract class CaseCompleted : Message
    {
        protected CaseCompleted(Case @case, TimeSpan duration)
        {
            Test = @case.Test;
            Name = @case.Name;
            Duration = duration;
            Output = @case.Output;
        }

        public Test Test { get; }
        public string Name { get; }
        public TimeSpan Duration { get; }
        public string Output { get; }
    }
}