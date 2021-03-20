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
    }
}