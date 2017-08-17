namespace Fixie.Execution
{
    using System;

    public class ClassCompleted : Message
    {
        public ClassCompleted(Type @class, ExecutionSummary summary)
        {
            Class = @class;
            Summary = summary;
        }

        public Type Class { get; }
        public ExecutionSummary Summary { get; }
    }
}