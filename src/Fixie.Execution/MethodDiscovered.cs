namespace Fixie.Execution
{
    using System;
    using System.Reflection;

    public class MethodDiscovered : Message
    {
        public MethodDiscovered(Type @class, MethodInfo method)
        {
            Class = @class;
            Method = method;
        }

        public Type Class { get; }
        public MethodInfo Method { get; }
    }
}