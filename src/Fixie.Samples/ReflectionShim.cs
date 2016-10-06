namespace Fixie.Samples
{
#if NET45
    using System;

    static class ReflectionShim
    {
        public static bool IsGenericType(this Type type)
            => type.IsGenericType;
    }
#else
    using System;
    using System.Reflection;

    static class ReflectionShim
    {
        public static bool IsGenericType(this Type type)
            => type.GetTypeInfo().IsGenericType;

        public static ConstructorInfo[] GetConstructors(this Type type)
            => type.GetTypeInfo().GetConstructors();
    }
#endif
}