using System;
using System.Reflection;

namespace Fixie.Execution
{
    [Serializable]
    public class AssemblyCompleted : Message
    {
        public AssemblyCompleted(Assembly assembly, AssemblyResult result)
        {
            Location = assembly.Location;
            Result = result;
        }

        public string Location { get; }
        public AssemblyResult Result { get; private set; }
    }
}