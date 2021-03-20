namespace Fixie
{
    using System.Reflection;

    public class TestName
    {
        public string FullName { get; }

        internal TestName(MethodInfo method)
        {
            FullName = method.ReflectedType!.FullName! + "." + method.Name;
        }
    }
}