namespace Fixie
{
    using System;
    using System.Reflection;

    static class ReflectionShim
    {
        public static Assembly Assembly(this Type type)
            => type.GetTypeInfo().Assembly;

        public static bool IsEnum(this Type conversionType)
            => conversionType.GetTypeInfo().IsEnum;

        public static bool IsValueType(this Type type)
            => type.GetTypeInfo().IsValueType;
    }
}