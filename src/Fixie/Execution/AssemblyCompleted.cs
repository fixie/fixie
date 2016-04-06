using System;

namespace Fixie.Execution
{
    [Serializable]
    public class AssemblyCompleted
    {
        public AssemblyCompleted(AssemblyInfo assembly, AssemblyResult result)
        {
            Assembly = assembly;
            Result = result;
        }

        public AssemblyInfo Assembly { get; private set; }
        public AssemblyResult Result { get; private set; }
    }
}