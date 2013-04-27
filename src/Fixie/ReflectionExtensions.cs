using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Fixie
{
    public static class ReflectionExtensions
    {
        public static bool Void(this MethodInfo method)
        {
            return method.ReturnType == typeof(void);
        }

        public static bool Has<TAttribute>(this MethodInfo method) where TAttribute : Attribute
        {
            return method.GetCustomAttributes<TAttribute>(false).Any();
        }

        public static bool Async(this MethodInfo method)
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
    }
}