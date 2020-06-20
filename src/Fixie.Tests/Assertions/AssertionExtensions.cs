namespace Fixie.Tests.Assertions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using static System.Environment;

    public static class AssertionExtensions
    {
        static readonly JsonSerializerOptions JsonSerializerOptions;

        static AssertionExtensions()
        {
            JsonSerializerOptions = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            JsonSerializerOptions.Converters.Add(new StringRepresentation<Type>());
        }

        class StringRepresentation<T> : JsonConverter<T>
        {
            public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
                => throw new NotImplementedException();

            public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
            {
                if (value is null)
                    writer.WriteNullValue();
                else
                    writer.WriteStringValue(value.ToString());
            }
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

        public static void ShouldBe<T>(this IEnumerable<T> actual, params T[] expected)
        {
            actual.ToArray().ShouldMatch(expected);
        }

        public static T ShouldBe<T>(this object? actual)
        {
            if (actual is T typed)
                return typed;

            throw new AssertException(typeof(T), actual?.GetType());
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

        public static void ShouldBeGreaterThan<T>(this T actual, T minimum) where T: IComparable<T>
        {
            if (actual.CompareTo(minimum) <= 0)
                throw new AssertException(ComparisonFailure(actual, minimum, ">"));
        }

        public static void ShouldBeGreaterThanOrEqualTo<T>(this T actual, T minimum) where T: IComparable<T>
        {
            if (actual.CompareTo(minimum) < 0)
                throw new AssertException(ComparisonFailure(actual, minimum, ">="));
        }

        static string ComparisonFailure(object left, object right, string operation)
        {
            return $"Expected: {left} {operation} {right}{NewLine}but it was not";
        }

        public static void ShouldMatch<T>(this T actual, T expected)
        {
            var actualJson = Json(actual);
            var expectedJson = Json(expected);
            
            if (actualJson != expectedJson)
                throw new MatchException(expectedJson, actualJson);
        }

        static string Json<T>(T @object)
        {
            return JsonSerializer.Serialize(@object, JsonSerializerOptions);
        }
    }
}