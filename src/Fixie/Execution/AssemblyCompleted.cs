﻿using System.Reflection;

namespace Fixie.Execution
{
    public class AssemblyCompleted
    {
        public AssemblyCompleted(Assembly assembly, AssemblyResult result)
        {
            Assembly = assembly;
            Result = result;
        }

        public Assembly Assembly { get; }
        public AssemblyResult Result { get; }
    }
}