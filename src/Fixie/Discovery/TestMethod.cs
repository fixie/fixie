using System;
using System.Reflection;

namespace Fixie.Discovery
{
    [Serializable]
    public class TestMethod
    {
        public string Class { get; private set; }
        public string Method { get; private set; }
        public string MethodGroup { get; private set; }

        public TestMethod(MethodInfo method)
        {
            Class = method.ReflectedType.FullName;
            Method = method.Name;
            MethodGroup = method.MethodGroup();
        }

        public TestMethod(string methodGroup)
        {
            var indexOfMemberSeparator = methodGroup.LastIndexOf(".");
            var className = methodGroup.Substring(0, indexOfMemberSeparator);
            var methodName = methodGroup.Substring(indexOfMemberSeparator + 1);

            Class = className;
            Method = methodName;
            MethodGroup = methodGroup;
        }
    }
}