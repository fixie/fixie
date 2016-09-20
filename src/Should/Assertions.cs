using Should.Core.Assertions;

namespace Should
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using Core.Exceptions;
    using static System.Environment;

    public static class Assertions
    {
        public static void ShouldBeFalse(this bool condition)
        {
            condition.ShouldEqual(false);
        }

        public static void ShouldBeTrue(this bool condition)
        {
            condition.ShouldEqual(true);
        }

        public static void ShouldBeTrue(this bool condition, string userMessage)
        {
            condition.ShouldEqual(true, userMessage);
        }

        public static void ShouldBeGreaterThan<T>(this T @object, T value)
        {
            var comparer = new AssertComparer<T>();
            if (comparer.Compare(@object, value) <= 0)
                throw new AssertException(ComparisonFailure(@object, value, ">"));
        }

        public static void ShouldBeGreaterThanOrEqualTo<T>(this T @object, T value)
        {
            var comparer = new AssertComparer<T>();
            if (comparer.Compare(@object, value) < 0)
                throw new AssertException(ComparisonFailure(@object, value, ">="));
        }

        static string ComparisonFailure(object left, object right, string operation)
        {
            return $"Assertion Failure:{NewLine}Expected: {Format(left)} {operation} {Format(right)}{NewLine}but it was not";
        }

        public static void ShouldBeNull(this object actual)
        {
            actual.ShouldEqual(null);
        }

        public static void ShouldBeType<T>(this object actual)
        {
            (actual?.GetType()).ShouldEqual(typeof(T));
        }

        public static void ShouldEqual<T>(this T actual, T expected)
        {
            Assert.Equal(expected, actual);
        }

        public static void ShouldEqual<T>(this T actual, T expected, string userMessage)
        {
            var comparer = new AssertEqualityComparer<T>();
            if (!comparer.Equals(expected, actual))
                throw new AssertActualExpectedException(expected, actual, userMessage);
        }

        public static void ShouldNotBeNull<T>(this T @object) where T : class
        {
            @object.ShouldNotEqual(null);
        }

        public static void ShouldNotEqual<T>(this T actual, T expected)
        {
            var comparer = new AssertEqualityComparer<T>();
            if (comparer.Equals(expected, actual))
                throw new AssertException($"Assertion Failure{NewLine}Unexpected: {Format(expected)}");
        }

        public static void ShouldBeEmpty(this string actual)
        {
            actual.ShouldEqual("");
        }

        public static void ShouldBeEmpty<T>(this IEnumerable<T> collection)
        {
            if (collection.Any())
                throw new AssertException($"Assertion Failure{NewLine}Collection was not empty.");
        }

        public static void ShouldContain<T>(this IEnumerable<T> collection, T expected)
        {
            if (!collection.Contains(expected))
                throw new AssertException($"Assertion Failure{NewLine}Collection does not contain expected item: {Format(expected)}");
        }

        static string Format(object value)
        {
            return value?.ToString() ?? "(null)";
        }
    }
}