namespace Fixie.Execution
{
    using System;
    using System.Reflection;

    [Serializable]
    public class AssemblyCompleted : Message
    {
        public AssemblyCompleted(Assembly assembly, AssemblyReport result)
        {
            Name = assembly.GetName().Name;
            Location = assembly.Location;
            Result = result;
        }

        public string Name { get; private set; }
        public string Location { get; private set; }
        public AssemblyReport Result { get; private set; }
    }
}