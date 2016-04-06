using System;
using System.Reflection;

namespace Fixie.Execution
{
    [Serializable]
    public class AssemblyInfo : Message
    {
        public AssemblyInfo(Assembly assembly)
        {
            Name = assembly.GetName().Name;
            Location = assembly.Location;
        }

        public string Name { get; private set; }
        public string Location { get; private set; }
    }
}