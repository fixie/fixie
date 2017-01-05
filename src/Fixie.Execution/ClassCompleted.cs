namespace Fixie.Execution
{
    using System;

    public class ClassCompleted : Message
    {
        public ClassCompleted(Type testClass)
        {
            Class = testClass;
        }

        public Type Class { get; }
    }
}