using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Fixie
{
    public static class ReflectionExtensions
    {
        public static bool IsVoid(this MethodInfo method)
        {
            return method.ReturnType == typeof(void);
        }

        public static bool Has<TAttribute>(this MemberInfo member) where TAttribute : Attribute
        {
            return member.GetCustomAttributes<TAttribute>(false).Any();
        }

        public static bool HasOrInherits<TAttribute>(this MemberInfo member) where TAttribute : Attribute
        {
            return member.GetCustomAttributes<TAttribute>(true).Any();
        }

        public static IEnumerable<TAttribute> GetCustomAttributes<TAttribute>(this MemberInfo member, bool inherit = false)
        {
            return member.GetCustomAttributes(typeof(TAttribute), inherit).Cast<TAttribute>();
        }

        public static bool IsAsync(this MethodInfo method)
        {
            return method.GetCustomAttributes(false).Any(x => x.GetType().FullName == "System.Runtime.CompilerServices.AsyncStateMachineAttribute");
        }

        public static bool IsDispose(this MethodInfo method)
        {
            var hasDisposeSignature = method.Name == "Dispose" && method.IsVoid() && method.GetParameters().Length == 0;

            if (!hasDisposeSignature)
                return false;

            return method.ReflectedType.GetInterfaces().Any(type => type == typeof(IDisposable));
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

        public static string FileName(this Assembly assembly)
        {
            return Path.GetFileName(assembly.Location);
        }
    }
}