namespace Fixie.Samples
{
    using System;
    using System.Reflection;

    static class ReflectionShim
    {
        public static bool IsGenericType(this Type type)
            => type.GetTypeInfo().IsGenericType;
    }
}