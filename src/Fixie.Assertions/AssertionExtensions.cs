namespace Fixie.Assertions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using static System.Environment;

    public static class AssertionExtensions
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

        public static void ShouldBeGreaterThan(this int actual, int minimum)
        {
            if (actual <= minimum)
                throw new AssertException(ComparisonFailure(actual, minimum, ">"));
        }

        public static void ShouldBeGreaterThanOrEqualTo(this int actual, int minimum)
        {
            if (actual < minimum)
                throw new AssertException(ComparisonFailure(actual, minimum, ">="));
        }

        public static void ShouldBeGreaterThanOrEqualTo(this TimeSpan actual, TimeSpan minimum)
        {
            if (actual < minimum)
                throw new AssertException(ComparisonFailure(actual, minimum, ">="));
        }

        static string ComparisonFailure(object left, object right, string operation)
        {
            return $"Expected: {Format(left)} {operation} {Format(right)}{NewLine}but it was not";
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
            var comparer = new AssertEqualityComparer<T>();
            if (!comparer.Equals(expected, actual))
                throw new AssertActualExpectedException(expected, actual);
        }

        public static void ShouldEqual<T>(this IEnumerable<T> actual, params T[] expected)
        {
            actual.ToArray().ShouldEqual(expected);
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
                throw new AssertException($"Unexpected: {Format(expected)}");
        }

        public static void ShouldBeEmpty(this string actual)
        {
            actual.ShouldEqual("");
        }

        public static void ShouldBeEmpty<T>(this IEnumerable<T> collection)
        {
            if (collection.Any())
                throw new AssertException("Collection was not empty.");
        }

        public static void ShouldContain<T>(this IEnumerable<T> collection, T expected)
        {
            if (!collection.Contains(expected))
                throw new AssertException($"Collection does not contain expected item: {Format(expected)}");
        }

        static string Format(object value)
        {
            return value?.ToString() ?? "(null)";
        }

        public static TException ShouldThrow<TException>(this Action shouldThrow, string expectedMessage) where TException : Exception
        {
            bool threw = false;
            Exception exception = null;

            try
            {
                shouldThrow();
            }
            catch (Exception actual)
            {
                threw = true;
                actual.ShouldBeType<TException>();
                actual.Message.ShouldEqual(expectedMessage);
                exception = actual;
            }

            threw.ShouldBeTrue();
            return (TException)exception;
        }
    }
}