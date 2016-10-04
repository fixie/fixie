namespace Fixie
{
#if NET45
    using System;
    using System.Reflection;

    static class ReflectionShim
    {
        public static Assembly Assembly(this Type type)
            => type.Assembly;

        public static bool IsEnum(this Type conversionType)
            => conversionType.IsEnum;

        public static bool IsValueType(this Type type)
            => type.IsValueType;
    }
#elif NETSTANDARD1_3
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    static class ReflectionShim
    {
        public static Assembly Assembly(this Type type)
            => type.GetTypeInfo().Assembly;

        public static bool IsEnum(this Type conversionType)
            => conversionType.GetTypeInfo().IsEnum;

        public static bool IsValueType(this Type type)
            => type.GetTypeInfo().IsValueType;

        public static IEnumerable<T> GetCustomAttributes<T>(this Type type, bool inherit) where T : Attribute
            => type.GetTypeInfo().GetCustomAttributes<T>(inherit);

        public static PropertyInfo GetProperty(this Type type, String name)
            => TypeExtensions.GetProperty(type, name);
    }
#endif
}