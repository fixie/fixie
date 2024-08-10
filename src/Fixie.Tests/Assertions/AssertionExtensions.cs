using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fixie.Tests.Assertions;

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

    public static void ShouldBe(this string? actual, string? expected)
    {
        if (actual != expected)
            throw new AssertException(expected, actual);
    }

    public static void ShouldBe<T>(this IEquatable<T> actual, IEquatable<T> expected)
    {
        if (!actual.Equals(expected))
            throw new AssertException(expected.ToString(), actual.ToString());
    }

    public static void ShouldBe(this object? actual, object? expected)
    {
        if (!Equals(actual, expected))
            throw new AssertException(expected?.ToString(), actual?.ToString());
    }

    public static void ShouldBe<T>(this IEnumerable<T> actual, T[] expected)
    {
        actual.ToArray().ShouldMatch(expected);
    }

    public static T ShouldBe<T>(this object? actual)
    {
        if (actual is T typed)
            return typed;

        throw new AssertException(typeof(T).ToString(), actual?.GetType().ToString());
    }

    public static void ShouldBeEmpty<T>(this IEnumerable<T> collection)
    {
        collection.ShouldMatch([]);
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

        throw new AssertException(typeof(TException).FullName, "No exception was thrown.");
    }

    public static async Task<TException> ShouldThrowAsync<TException>(this Func<Task> shouldThrowAsync, string expectedMessage) where TException : Exception
    {
        try
        {
            await shouldThrowAsync();
        }
        catch (Exception actual)
        {
            actual
                .ShouldBe<TException>()
                .Message.ShouldBe(expectedMessage);
            return (TException)actual;
        }

        throw new AssertException(typeof(TException).FullName, "No exception was thrown.");
    }

    public static void ShouldBeGreaterThan<T>(this T actual, T minimum) where T: IComparable<T>
    {
        if (actual.CompareTo(minimum) <= 0)
            throw new AssertException($"value > {minimum}", actual.ToString());
    }

    public static void ShouldBeGreaterThanOrEqualTo<T>(this T actual, T minimum) where T: IComparable<T>
    {
        if (actual.CompareTo(minimum) < 0)
            throw new AssertException($"value >= {minimum}", actual.ToString());
    }

    public static void ShouldMatch<T>(this T actual, T expected)
    {
        var actualJson = Json(actual);
        var expectedJson = Json(expected);
            
        if (actualJson != expectedJson)
            throw new AssertException(expectedJson, actualJson);
    }

    public static void ShouldSatisfy<T>(this IEnumerable<T> actual, Action<T>[] itemExpectations)
    {
        var actualItems = actual.ToArray();

        if (actualItems.Length != itemExpectations.Length)
            throw new AssertException(
                $"{itemExpectations.Length} items",
                $"{actualItems.Length} items");

        for (var i = 0; i < actualItems.Length; i++)
            itemExpectations[i](actualItems[i]);
    }

    public static void ShouldBeGenericTypeParameter(this Type actual, string expectedName)
    {
        actual.IsGenericParameter.ShouldBe(true);
        actual.FullName.ShouldBe(null);
        actual.Name.ShouldBe(expectedName);
    }

    public static void ShouldNotBeNull([NotNull] this object? actual)
    {
        if (actual == null)
            throw new AssertException("not null", "null");
    }

    static string Json<T>(T @object)
    {
        return JsonSerializer.Serialize(@object, JsonSerializerOptions);
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
}