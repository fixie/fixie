namespace Fixie
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    public static class ReflectionExtensions
    {
        public static bool IsVoid(this MethodInfo method)
        {
            return method.ReturnType == typeof(void);
        }

        public static bool Has<TAttribute>(this Type type) where TAttribute : Attribute
        {
            return type.GetCustomAttributes<TAttribute>(false).Any();
        }

        public static bool HasOrInherits<TAttribute>(this Type type) where TAttribute : Attribute
        {
            return type.GetCustomAttributes<TAttribute>(true).Any();
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

        public static bool HasSignature(this MethodInfo method, Type returnType, string name, params Type[] parameterTypes)
        {
            if (method.Name != name)
                return false;

            if (method.ReturnType != returnType)
                return false;

            var parameters = method.GetParameters();

            if (parameters.Length != parameterTypes.Length)
                return false;

            for (int i = 0; i < parameters.Length; i++)
                if (parameters[i].ParameterType != parameterTypes[i])
                    return false;

            return true;
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