namespace Fixie.Execution
{
    using System;

    public class ClassStarted : Message
    {
        public ClassStarted(Type testClass)
        {
            Class = testClass;
        }

        public Type Class { get; }
    }
}