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
            return type.GetTypeInfo().GetCustomAttributes<TAttribute>(false).Any();
        }

        public static bool HasOrInherits<TAttribute>(this Type type) where TAttribute : Attribute
        {
            return type.GetTypeInfo().GetCustomAttributes<TAttribute>(true).Any();
        }

        public static bool Has<TAttribute>(this MethodInfo method) where TAttribute : Attribute
        {
            return method.GetCustomAttributes<TAttribute>(false).Any();
        }

        public static bool HasOrInherits<TAttribute>(this MethodInfo method) where TAttribute : Attribute
        {
            return method.GetCustomAttributes<TAttribute>(true).Any();
        }

        public static bool IsAsync(this MethodInfo method)
        {
            return method.Has<AsyncStateMachineAttribute>();
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