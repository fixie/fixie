namespace Fixie
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
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

        public static async Task DisposeIfApplicableAsync(this object? o)
        {
            if (o is IAsyncDisposable asyncDisposable)
                await asyncDisposable.DisposeAsync();

            if (o is IDisposable disposable)
                disposable.Dispose();
        }
    }
}