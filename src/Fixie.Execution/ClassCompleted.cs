namespace Fixie.Execution
{
    using System;

    public class ClassCompleted : Message
    {
        public ClassCompleted(Type @class)
        {
            Class = @class;
        }

        public Type Class { get; }
    }
}