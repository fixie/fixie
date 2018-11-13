namespace Fixie
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    public static class ReflectionExtensions
    {
        public static string TypeName(this object o)
        {
            return o?.GetType().FullName;
        }

        public static bool IsVoid(this MethodInfo method)
        {
            return method.ReturnType == typeof(void);
        }

        public static bool IsStatic(this Type type)
        {
            return type.IsAbstract && type.IsSealed;
        }

        public static bool Has<TAttribute>(this Type type) where TAttribute : Attribute
        {
            return type.GetCustomAttributes<TAttribute>(true).Any();
        }

        public static bool Has<TAttribute>(this MethodInfo method) where TAttribute : Attribute
        {
            return method.GetCustomAttributes<TAttribute>(true).Any();
        }

        public static bool IsAsync(this MethodInfo method)
        {
            var returnType = method.ReturnType;
            return method.Has<AsyncStateMachineAttribute>() ||
                returnType == typeof(System.Threading.Tasks.Task) ||
                returnType.IsFSharpAsync();
        }

        internal static bool IsFSharpAsync(this Type returnType) {
            return returnType.IsGenericType &&
                returnType.GetGenericTypeDefinition().FullName == "Microsoft.FSharp.Control.FSharpAsync`1";
        }

        public static bool IsInNamespace(this Type type, string ns)
        {
            var actual = type.Namespace;

            if (ns == null)
                return actual == null;

            if (actual == null)
                return false;

            return actual == ns || actual.StartsWith(ns + ".");
        }

        public static void Dispose(this object o)
        {
            (o as IDisposable)?.Dispose();
        }
    }
}