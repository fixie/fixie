using System;
using System.Reflection;

namespace Fixie.Execution
{
    [Serializable]
    public class AssemblyCompleted : Message
    {
        public AssemblyCompleted(Assembly assembly, AssemblyReport result)
        {
            Location = assembly.Location;
            Result = result;
        }

        public string Location { get; }
        public AssemblyReport Result { get; private set; }
    }
}