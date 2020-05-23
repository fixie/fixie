namespace Fixie
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using static Internal.Maybe;

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

        public static bool Has<TAttribute>(this MemberInfo member, [NotNullWhen(true)] out TAttribute? matchingAttribute) where TAttribute : Attribute
        {
            return Try(() => member.GetCustomAttribute<TAttribute>(true), out matchingAttribute);
        }

        public static void Dispose(this object? o)
        {
            (o as IDisposable)?.Dispose();
        }
    }
}