namespace Fixie
{
    using System.Reflection;

    public class TestName
    {
        public string Class { get; }
        public string Method { get; }
        public string FullName { get; }

        internal TestName(MethodInfo method)
        {
            Class = method.ReflectedType!.FullName!;
            Method = method.Name;
            FullName = Class + "." + Method;
        }

        internal TestName(string fullName)
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