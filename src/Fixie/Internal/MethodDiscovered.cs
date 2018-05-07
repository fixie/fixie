namespace Fixie.Internal
{
    using System.Reflection;

    public class MethodDiscovered : Message
    {
        public MethodDiscovered(MethodInfo method)
            => Method = method;

        public MethodInfo Method { get; }
    }
}