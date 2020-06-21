namespace Fixie.Internal
{
    using System.Reflection;

    public class TestDiscovered : Message
    {
        public TestDiscovered(MethodInfo method)
            => Method = method;

        public MethodInfo Method { get; }
    }
}