namespace Fixie
{
    using System;
    using System.Linq;
    using System.Reflection;

    public static class ReflectionExtensions
    {
        public static bool IsVoid(this MethodInfo method)
        {
            return method.ReturnType == typeof(void);
        }

        public static bool IsStatic(this Type type)
        {
            return type.IsAbstract && type.IsSealed;
        }

        public static bool Has<TAttribute>(this MemberInfo member) where TAttribute : Attribute
        {
            return member.GetCustomAttributes<TAttribute>(true).Any();
        }

        public static void Dispose(this object? o)
        {
            (o as IDisposable)?.Dispose();
        }
    }
}