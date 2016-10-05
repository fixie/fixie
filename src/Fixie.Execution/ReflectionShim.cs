namespace Fixie.Execution
{
#if NET45
    using System;

    static class ReflectionShim
    {
        public static bool IsClass(this Type type)
            => type.IsClass;

        public static bool IsAbstract(this Type type)
            => type.IsAbstract;

        public static bool IsGenericType(this Type type)
            => type.IsGenericType;
    }
#elif NETSTANDARD1_5
    using System;
    using System.Reflection;

    static class ReflectionShim
    {
        public static bool IsClass(this Type type)
            => type.GetTypeInfo().IsClass;

        public static bool IsAbstract(this Type type)
            => type.GetTypeInfo().IsAbstract;

        public static bool IsGenericType(this Type type)
            => type.GetTypeInfo().IsGenericType;

        public static bool IsSubclassOf(this Type type, Type c)
            => type.GetTypeInfo().IsSubclassOf(c);
    }
#endif
}