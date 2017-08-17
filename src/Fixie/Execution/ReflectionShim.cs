namespace Fixie.Execution
{
    using System;
    using System.Reflection;

    static class ReflectionShim
    {
        public static bool IsAbstract(this Type type)
            => type.GetTypeInfo().IsAbstract;

        public static bool IsClass(this Type type)
            => type.GetTypeInfo().IsClass;

        public static bool IsGenericType(this Type type)
            => type.GetTypeInfo().IsGenericType;

        public static bool IsSubclassOf(this Type type, Type c)
            => type.GetTypeInfo().IsSubclassOf(c);
    }
}