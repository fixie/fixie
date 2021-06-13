namespace Fixie
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using static Internal.Maybe;

    public static class ReflectionExtensions
    {
        internal static string TestName(this MethodInfo method)
        {
            return method.ReflectedType!.FullName! + "." + method.Name;
        }

        internal static bool IsStatic(this Type type)
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
    }
}