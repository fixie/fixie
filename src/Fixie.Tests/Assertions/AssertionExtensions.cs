namespace Fixie.Tests.Assertions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using static System.Environment;

    public static class AssertionExtensions
    {
        static readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };

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

        public static void ShouldBe(this IEnumerable<Type> actual, params Type[] expected)
        {
            var actualArray = actual.ToArray();
        
            if (actualArray.Length != expected.Length)
                throw new AssertException(expected, actualArray);
        
            for (var i = 0; i < actualArray.Length; i++)
                if (actualArray[i] != expected[i])
                    throw new AssertException(expected, actualArray);
        }
        
        public static void ShouldBe(this IEnumerable<string> actual, params string[] expected)
        {
            actual.ToArray().ShouldBe(expected);
        }
        
        public static void ShouldBe(this string[] actual, string[] expected)
        {
            if (actual.Length != expected.Length)
                throw new AssertException(expected, actual);
        
            for (var i = 0; i < actual.Length; i++)
                if (actual[i] != expected[i])
                    throw new AssertException(expected, actual);
        }

        public static void ShouldMatch<T>(this T actual, T expected)
        {
            var actualJson = JsonSerializer.Serialize(actual, JsonSerializerOptions);
            var expectedJson = JsonSerializer.Serialize(expected, JsonSerializerOptions);

            actualJson.ShouldBe(expectedJson);
        }

        public static void ShouldMatch<T>(this IEnumerable<T> actual, params T[] expected)
        {
            actual.ToArray().ShouldMatch(expected);
        }

        public static void ShouldBe(this string? actual, string? expected)
        {
            if (actual != expected)
                throw new AssertException(expected, actual);
        }

        public static void ShouldBe<T>(this IEquatable<T> actual, IEquatable<T> expected)
        {
            if (!actual.Equals(expected))
                throw new AssertException(expected, actual);
        }

        public static void ShouldBe(this object? actual, object? expected)
        {
            if (!Equals(actual, expected))
                throw new AssertException(expected, actual);
        }

        public static void ShouldBe(this object? actual, object? expected, string userMessage)
        {
            if (!Equals(actual, expected))
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
    }
}