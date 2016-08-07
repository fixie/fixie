namespace Fixie.Execution
{
    using System;

    public class ClassStarted : Message
    {
        public ClassStarted(Type testClass)
        {
            FullName = testClass.FullName;
        }

        public string FullName { get; }
    }
}