using System;
using System.Reflection;

namespace Fixie
{
    [Serializable]
    public class MethodGroup
    {
        public string Class { get; private set; }
        public string Method { get; private set; }
        public string FullName { get; private set; }

        public MethodGroup(MethodInfo method)
        {
            Class = method.ReflectedType.FullName;
            Method = method.Name;
            FullName = Class + "." + Method;
        }

        public MethodGroup(string fullName)
        {
            var indexOfMemberSeparator = fullName.LastIndexOf(".");
            var className = fullName.Substring(0, indexOfMemberSeparator);
            var methodName = fullName.Substring(indexOfMemberSeparator + 1);

            Class = className;
            Method = methodName;
            FullName = fullName;
        }
    }
}