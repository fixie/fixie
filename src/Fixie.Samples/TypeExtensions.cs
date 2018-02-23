namespace Fixie.Samples
{
    using System;
    using System.Linq;
    using System.Reflection;

    public static class TypeExtensions
    {
        public static void Execute(this Type testClass, Func<MethodInfo, bool> condition, object instance)
        {
            var query = testClass
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(condition);

            foreach (var q in query)
                q.Execute(instance);
        }
    }
}