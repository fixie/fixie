namespace Fixie.Tests
{
#if NET45
    using System;
    using System.Reflection;

    static class ReflectionShim
    {
        public static Assembly Assembly(this Type type)
            => type.Assembly;

        public static bool IsValueType(this Type type)
            => type.IsValueType;

        public static bool IsNestedPrivate(this Type type)
            => type.IsNestedPrivate;
    }
#else
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    static class ReflectionShim
    {
        public static Assembly Assembly(this Type type)
            => type.GetTypeInfo().Assembly;

        public static bool IsValueType(this Type type)
            => type.GetTypeInfo().IsValueType;

        public static bool IsNestedPrivate(this Type type)
            => type.GetTypeInfo().IsNestedPrivate;

        public static T GetCustomAttribute<T>(this Type type, bool inherit) where T : Attribute
            => type.GetTypeInfo().GetCustomAttribute<T>(inherit);

        public static PropertyInfo[] GetProperties(this Type type)
            => type.GetTypeInfo().GetProperties();
    }
#endif
}