namespace Fixie.Execution
{
    using System;
    using System.Reflection;

    [Serializable]
    public class AssemblyCompleted : Message
    {
        public AssemblyCompleted(Assembly assembly)
        {
            Name = assembly.GetName().Name;
            Location = assembly.Location;
        }

        public string Name { get; private set; }
        public string Location { get; private set; }
    }
}