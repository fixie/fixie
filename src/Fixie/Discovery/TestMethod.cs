using System;
using System.Reflection;

namespace Fixie.Discovery
{
    [Serializable]
    public class TestMethod
    {
        public string Class { get; private set; }
        public string Method { get; private set; }
        public string FullName { get; private set; }

        public TestMethod(MethodInfo method)
        {
            Class = method.ReflectedType.FullName;
            Method = method.Name;
            FullName = Class + "." + Method;
        }
    }
}