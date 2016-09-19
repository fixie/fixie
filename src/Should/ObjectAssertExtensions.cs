using Should.Core.Assertions;

namespace Should
{
    using System;
    using Core.Exceptions;

    public static class ObjectAssertExtensions
    {
        public static void ShouldBeGreaterThan<T>(this T @object, T value)
        {
            var comparer = new AssertComparer<T>();
            if (comparer.Compare(@object, value) <= 0)
                throw new GreaterThanException(@object, value);
        }

        public static void ShouldBeGreaterThanOrEqualTo<T>(this T @object, T value)
        {
            var comparer = new AssertComparer<T>();
            if (comparer.Compare(@object, value) < 0)
                throw new GreaterThanOrEqualException(@object, value);
        }

        public static void ShouldBeNull(this object @object)
        {
            if (@object != null)
                throw new NullException(@object);
        }

        public static T ShouldBeType<T>(this object @object)
        {
            Type expectedType = typeof(T);
            if (@object == null || !expectedType.Equals(@object.GetType()))
                throw new IsTypeException(expectedType, @object);
            return (T)@object;
        }

        public static void ShouldEqual<T>(this T actual,
                                          T expected)
        {
            Assert.Equal(expected, actual);
        }

        public static void ShouldEqual<T>(this T actual,
                                          T expected,
                                          string userMessage)
        {
            var comparer = new AssertEqualityComparer<T>();
            if (!comparer.Equals(expected, actual))
                throw new EqualException(expected, actual, userMessage);
        }

        public static T ShouldNotBeNull<T>(this T @object) where T : class
        {
            if (@object == null)
                throw new NotNullException();
            return @object;
        }

        public static void ShouldNotEqual<T>(this T actual,
                                             T expected)
        {
            var comparer = new AssertEqualityComparer<T>();
            if (comparer.Equals(expected, actual))
                throw new NotEqualException(expected, actual);
        }
    }
}