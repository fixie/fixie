namespace Fixie.Execution
{
    public class MethodGroupDiscovered : Message
    {
        public MethodGroupDiscovered(MethodGroup methodGroup)
        {
            MethodGroup = methodGroup;
        }

        public MethodGroup MethodGroup { get; }
    }
}