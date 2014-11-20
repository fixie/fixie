using System;
using System.Reflection;

namespace Fixie.Discovery
{
    [Serializable]
    public class TestMethod
    {
        public Type Class { get; private set; }
        public MethodInfo Method { get; private set; }
        public string FullName { get; private set; }

        public TestMethod(MethodInfo method)
        {
            Class = method.ReflectedType;
            Method = method;
            FullName = Class.FullName + "." + Method.Name;
        }
    }
}