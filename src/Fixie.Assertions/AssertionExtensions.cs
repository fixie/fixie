namespace Fixie.Assertions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using static System.Environment;

    public static class AssertionExtensions
    {
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

        public static void ShouldBeType<T>(this object actual)
        {
            (actual?.GetType()).ShouldBe(typeof(T));
        }

        public static void ShouldBe<T>(this IEnumerable<T> actual, params T[] expected)
        {
            actual.ToArray().ShouldBe(expected);
        }

        public static void ShouldBe(this bool actual, bool expected, string userMessage = null)
        {
            if (actual != expected)
                throw new ExpectedException(expected, actual, userMessage);
        }

        public static void ShouldBe(this int actual, int expected, string userMessage = null)
        {
            if (actual != expected)
                throw new ExpectedException(expected, actual, userMessage);
        }

        public static void ShouldBe(this string actual, string expected, string userMessage = null)
        {
            if (actual != expected)
                throw new ExpectedException(expected, actual, userMessage);
        }

        public static void ShouldBe<T>(this T? actual, T? expected, string userMessage = null) where T : struct
        {
            if (!Nullable.Equals(actual, expected))
                throw new ExpectedException(expected, actual, userMessage);
        }

        public static void ShouldBe<T>(this T actual, T expected, string userMessage = null)
        {
            if (!expected.Is(actual))
                throw new ExpectedException(expected, actual, userMessage);
        }

        public static void ShouldNotBe<T>(this T actual, T expected)
        {
            if (expected.Is(actual))
                throw new AssertException($"Unexpected: {Format(expected)}");
        }

        public static void ShouldBeEmpty<T>(this IEnumerable<T> collection)
        {
            if (collection.Any())
                throw new AssertException("Collection was not empty.");
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
                actual.Message.ShouldBe(expectedMessage);
                exception = actual;
            }

            threw.ShouldBe(true);
            return (TException)exception;
        }

        static bool Is<T>(this T x, T y)
        {
            var type = typeof(T);

            if (IsReferenceType(type) || IsNullableValueType(type))
            {
                if (Equals(x, default(T)))
                    return Equals(y, default(T));

                if (Equals(y, default(T)))
                    return false;
            }

            if (x is IEquatable<T> equatable)
                return equatable.Equals(y);

            if (x is IEnumerable enumerableX && y is IEnumerable enumerableY)
            {
                var enumeratorX = enumerableX.GetEnumerator();
                var enumeratorY = enumerableY.GetEnumerator();

                while (true)
                {
                    bool hasNextX = enumeratorX.MoveNext();
                    bool hasNextY = enumeratorY.MoveNext();

                    if (!hasNextX || !hasNextY)
                        return hasNextX == hasNextY;

                    var currentX = enumeratorX.Current;
                    var currentY = enumeratorY.Current;

                    if (!currentX.Is(currentY))
                        return false;
                }
            }

            return Equals(x, y);
        }

        static bool IsNullableValueType(Type type)
        {
            return Nullable.GetUnderlyingType(type) != null;
        }

        static bool IsReferenceType(Type type)
        {
            return !type.IsValueType;
        }
    }
}