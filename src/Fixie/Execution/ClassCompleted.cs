namespace Fixie.Execution
{
    using System;

    public class ClassCompleted : Message
    {
        public ClassCompleted(Type testClass)
        {
            FullName = testClass.FullName;
        }

        public string FullName { get; }
    }
}