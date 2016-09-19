using Should.Core.Assertions;

namespace Should
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Core.Exceptions;

    public static class Assertions
    {
        public static void ShouldBeFalse(this bool condition)
        {
            if (condition)
                throw new FalseException(null);
        }

        public static void ShouldBeFalse(this bool condition, string userMessage)
        {
            if (condition)
                throw new FalseException(userMessage);
        }

        public static void ShouldBeTrue(this bool condition)
        {
            if (!condition)
                throw new TrueException(null);
        }

        public static void ShouldBeTrue(this bool condition, string userMessage)
        {
            if (!condition)
                throw new TrueException(userMessage);
        }

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
            var expectedType = typeof(T);
            if (@object == null || !expectedType.Equals(@object.GetType()))
                throw new IsTypeException(expectedType, @object);
            return (T)@object;
        }

        public static void ShouldEqual<T>(this T actual, T expected)
        {
            Assert.Equal(expected, actual);
        }

        public static void ShouldEqual<T>(this T actual, T expected, string userMessage)
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

        public static void ShouldBeEmpty(this IEnumerable collection)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection), "cannot be null");

            foreach (var @object in collection)
                throw new EmptyException();
        }

        public static void ShouldContain<T>(this IEnumerable<T> collection, T expected)
        {
            var comparer = new AssertEqualityComparer<T>();

            foreach (var item in collection)
                if (comparer.Equals(expected, item))
                    return;

            throw new ContainsException(expected);
        }
    }
}