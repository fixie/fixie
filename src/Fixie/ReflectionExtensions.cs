namespace Fixie;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using static Internal.Maybe;

public static class ReflectionExtensions
{
    internal static string TestName(this MethodInfo method)
        => method.ReflectedType!.FullName! + "." + method.Name;

    internal static bool IsStatic(this Type type)
        => type.IsAbstract && type.IsSealed;

    /// <summary>
    /// Determines whether any attributes of the specified type are applied to a
    /// specified member including the ancestors of that member.
    /// </summary>
    public static bool Has<TAttribute>(this MemberInfo member) where TAttribute : Attribute
        => member.GetCustomAttributes<TAttribute>(true).Any();

    /// <summary>
    /// Determines whether an attribute of the specified type is applied to a
    /// specified member including the ancestors of that member, providing that
    /// attribute as an `out` parameter when found.
    /// </summary>
    public static bool Has<TAttribute>(this MemberInfo member, [NotNullWhen(true)] out TAttribute? matchingAttribute) where TAttribute : Attribute
        => Try(() => member.GetCustomAttribute<TAttribute>(true), out matchingAttribute);
}