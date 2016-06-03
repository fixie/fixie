namespace Fixie.Execution
{
    using System;

    [Serializable]
    public class ClassCompleted : Message
    {
        public ClassCompleted(Type testClass)
        {
            FullName = testClass.FullName;
        }

        public string FullName { get; }
    }
}