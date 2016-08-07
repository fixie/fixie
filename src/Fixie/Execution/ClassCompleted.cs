namespace Fixie.Execution
{
    using System;

    public class ClassCompleted : Message
    {
        public ClassCompleted(Type testClass)
        {
            TestClass = testClass;
        }

        public Type TestClass { get; }
    }
}