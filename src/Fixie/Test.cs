namespace Fixie
{
    using System.Reflection;

    public class Test
    {
        public string Class { get; }
        public string Method { get; }
        public string Name { get; }

        internal Test(MethodInfo method)
        {
            Class = method.ReflectedType!.FullName!;
            Method = method.Name;
            Name = Class + "." + Method;
        }

        internal Test(string @class, string method)
        {
            Class = @class;
            Method = method;
            Name = Class + "." + Method;
        }

        internal Test(string name)
        {
            var indexOfMemberSeparator = name.LastIndexOf(".");
            var className = name.Substring(0, indexOfMemberSeparator);
            var methodName = name.Substring(indexOfMemberSeparator + 1);

            Class = className;
            Method = methodName;
            Name = name;
        }
    }
}