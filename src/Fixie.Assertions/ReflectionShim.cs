namespace Fixie.Assertions
{
    using System;
    using System.Reflection;

    static class ReflectionShim
    {
        public static bool IsGenericType(this Type type)
            => type.GetTypeInfo().IsGenericType;

        public static bool IsValueType(this Type type)
            => type.GetTypeInfo().IsValueType;
    }
}