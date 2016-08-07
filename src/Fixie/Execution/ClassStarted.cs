namespace Fixie.Execution
{
    using System;

    public class ClassStarted : Message
    {
        public ClassStarted(Type testClass)
        {
            TestClass = testClass;
        }

        public Type TestClass { get; }
    }
}