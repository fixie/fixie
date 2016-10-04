namespace Fixie.Assertions
{
    using System;

    static class ReflectionShim
    {
        public static bool IsGenericType(this Type type)
            => type.IsGenericType;

        public static bool IsValueType(this Type type)
            => type.IsValueType;
    }
}
