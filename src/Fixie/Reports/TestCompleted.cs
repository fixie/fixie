namespace Fixie.Reports
{
    using System;
    using Internal;

    public abstract class TestCompleted : Message
    {
        internal TestCompleted(Case @case, TimeSpan duration, string output)
        {
            Test = @case.Test.Name;
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