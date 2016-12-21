namespace Fixie.Execution
{
    using System;
    using System.Reflection;

    [Serializable]
    public class AssemblyCompleted
    {
        public AssemblyCompleted(Assembly assembly, AssemblyResult result)
        {
            Name = assembly.GetName().Name;
            Location = assembly.Location;
            Result = result;
        }

        public string Name { get; private set; }
        public string Location { get; private set; }
        public AssemblyResult Result { get; private set; }
    }
}