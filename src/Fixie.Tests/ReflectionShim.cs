namespace Fixie.Tests
{
    using System;
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
            => CustomAttributeExtensions.GetCustomAttribute<T>(type.GetTypeInfo(), inherit);

        public static PropertyInfo[] GetProperties(this Type type)
            => type.GetTypeInfo().GetProperties();
    }
}