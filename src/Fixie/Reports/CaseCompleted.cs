namespace Fixie.Reports
{
    using System;
    using Internal;

    public abstract class CaseCompleted : Message
    {
        internal CaseCompleted(Case @case, TimeSpan duration, string output)
        {
            Test = @case.Test;
            Name = @case.Name;
            Duration = duration;
            Output = output;
        }

        public string Test { get; }
        public string Name { get; }
        public TimeSpan Duration { get; }
        public string Output { get; }
    }
}