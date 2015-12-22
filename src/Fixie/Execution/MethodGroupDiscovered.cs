using System;

namespace Fixie.Execution
{
    [Serializable]
    public class MethodGroupDiscovered : IMessage
    {
        public MethodGroup MethodGroup { get; }

        public MethodGroupDiscovered(MethodGroup methodGroup)
        {
            MethodGroup = methodGroup;
        }
    }
}