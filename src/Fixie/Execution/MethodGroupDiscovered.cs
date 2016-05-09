using System;

namespace Fixie.Execution
{
    [Serializable]
    public class MethodGroupDiscovered : Message
    {
        public MethodGroupDiscovered(MethodGroup methodGroup)
        {
            MethodGroup = methodGroup;
        }

        public MethodGroup MethodGroup { get; }
    }
}