namespace Fixie.Reports
{
    using System;

    public abstract class TestCompleted : Message
    {
        internal TestCompleted(string test, string name, TimeSpan duration, string output)
        {
            Test = test;
            Name = name;
            Duration = duration;
            Output = output;
        }

        public string Test { get; }
        public string Name { get; }
        public TimeSpan Duration { get; }
        public string Output { get; }
    }
}