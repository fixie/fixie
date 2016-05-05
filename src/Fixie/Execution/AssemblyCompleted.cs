using System;
using System.Reflection;

namespace Fixie.Execution
{
    [Serializable]
    public class AssemblyCompleted : Message
    {
        public AssemblyCompleted(Assembly assembly)
        {
            Location = assembly.Location;
        }

        public string Location { get; }
    }
}