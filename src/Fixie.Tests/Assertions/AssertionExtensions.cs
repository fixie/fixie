namespace Fixie.Tests.Assertions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Reflection;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
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

        public static void ShouldBeGreaterThanOrEqualTo(this double actual, double minimum)
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
            return $"Expected: {left} {operation} {right}{NewLine}but it was not";
        }

        public static T ShouldBe<T>(this object? actual)
        {
            if (actual is T typed)
                return typed;

            throw new AssertException(typeof(T), actual?.GetType());
        }

        public static void ShouldBe<T>(this IEnumerable<T> actual, params T[] expected)
        {
            actual.ToArray().ShouldBe(expected);
        }

        public static void ShouldBe(this bool actual, bool expected)
        {
            if (actual != expected)
                throw new AssertException(expected, actual);
        }

        public static void ShouldBe(this int actual, int expected)
        {
            if (actual != expected)
                throw new AssertException(expected, actual);
        }

        public static void ShouldBe(this string? actual, string? expected)
        {
            if (actual != expected)
                throw new AssertException(expected, actual);
        }

        public static void ShouldBe(this Assembly actual, Assembly expected)
        {
            if (actual != expected)
                throw new AssertException(expected, actual);
        }
        
        public static void ShouldBe(this Type actual, Type expected)
        {
            if (actual != expected)
                throw new AssertException(expected, actual);
        }

        public static void ShouldBe(this Type actual, Type expected, string userMessage)
        {
            if (actual != expected)
                throw new AssertException(expected, actual, userMessage);
        }
        
        public static void ShouldBe(this TimeSpan actual, TimeSpan expected)
        {
            if (actual != expected)
                throw new AssertException(expected, actual);
        }

        internal static void ShouldBe(this CaseState actual, CaseState expected)
        {
            if (actual != expected)
                throw new AssertException(expected, actual);
        }
        
        public static void ShouldBe(this TestOutcome actual, TestOutcome expected)
        {
            if (actual != expected)
                throw new AssertException(expected, actual);
        }

        public static void ShouldBe(this Exception? actual, Exception? expected)
        {
            if (actual != expected)
                throw new AssertException(expected, actual);
        }
        
        public static void ShouldBe(this HttpMethod actual, HttpMethod expected)
        {
            var x = actual as IEquatable<HttpMethod>;
            var y = expected as IEquatable<HttpMethod>;

            if (!x.Equals(y))
                throw new AssertException(expected, actual);
        }
        
        public static void ShouldBe<T>(this T? actual, T? expected) where T: Attribute
        {
            if (!Equals(actual, expected))
                throw new AssertException(expected, actual);
        }

        public static void ShouldBe<T>(this T actual, T expected, string? userMessage = null)
        {
            if (!expected.Is(actual))
                throw new AssertException(expected, actual, userMessage);
        }

        public static void ShouldBeEmpty<T>(this IEnumerable<T> collection)
        {
            if (collection.Any())
                throw new AssertException("Collection was not empty.");
        }

        public static TException ShouldThrow<TException>(this Action shouldThrow, string expectedMessage) where TException : Exception
        {
            try
            {
                shouldThrow();
            }
            catch (Exception actual)
            {
                actual
                    .ShouldBe<TException>()
                    .Message.ShouldBe(expectedMessage);
                return (TException)actual;
            }

            throw new AssertException("Expected an exception to be thrown.");
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