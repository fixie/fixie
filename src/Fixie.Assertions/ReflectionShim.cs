namespace Fixie.Assertions
{
#if NET45
    using System;

    static class ReflectionShim
    {
        public static bool IsGenericType(this Type type)
            => type.IsGenericType;

        public static bool IsValueType(this Type type)
            => type.IsValueType;
    }
#else
    using System;
    using System.Reflection;

    static class ReflectionShim
    {
        public static bool IsGenericType(this Type type)
            => type.GetTypeInfo().IsGenericType;

        public static bool IsValueType(this Type type)
            => type.GetTypeInfo().IsValueType;

        public static MethodInfo GetMethod(this Type type, String name)
           => TypeExtensions.GetMethod(type, name);

        public static MethodInfo GetMethod(this Type type, String name, Type[] types)
            => TypeExtensions.GetMethod(type, name, types);

        public static bool IsAssignableFrom(this Type type, Type c)
            => TypeExtensions.IsAssignableFrom(type, c);
    }
#endif
}