namespace Fixie
{
    using System.Reflection;

    public class TestMethod
    {
        public MethodInfo Method { get; }

        internal TestMethod(MethodInfo method)
            => Method = method;
    }
}