using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
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

    public static void ShouldBe(this string? actual, string? expected, [CallerArgumentExpression(nameof(actual))] string? expression = null)
    {
        if (actual != expected)
            throw new AssertException(expression, expected, actual);
    }

    public static void ShouldBe(this bool actual, bool expected, [CallerArgumentExpression(nameof(actual))] string? expression = null)
    {
        if (actual != expected)
            throw new AssertException(expression, Serialize(expected), Serialize(actual));
    }

    public static void ShouldBe<T>(this IEquatable<T> actual, IEquatable<T> expected, [CallerArgumentExpression(nameof(actual))] string? expression = null)
    {
        if (!actual.Equals(expected))
            throw new AssertException(expression, expected.ToString(), actual.ToString());
    }

    public static void ShouldBe(this object? actual, object? expected, [CallerArgumentExpression(nameof(actual))] string? expression = null)
    {
        if (!Equals(actual, expected))
            throw new AssertException(expression, expected?.ToString(), actual?.ToString());
    }

    public static void ShouldBe<T>(this IEnumerable<T> actual, T[] expected, [CallerArgumentExpression(nameof(actual))] string? expression = null)
    {
        actual.ToArray().ShouldMatch(expected, expression);
    }

    public static T ShouldBe<T>(this object? actual, [CallerArgumentExpression(nameof(actual))] string? expression = null)
    {
        if (actual is T typed)
            return typed;

        throw new AssertException(expression, typeof(T).ToString(), actual?.GetType().ToString());
    }

    public static void ShouldBeEmpty<T>(this IEnumerable<T> collection, [CallerArgumentExpression(nameof(collection))] string? expression = null)
    {
        collection.ShouldMatch([], expression);
    }

    public static TException ShouldThrow<TException>(this Action shouldThrow, string expectedMessage, [CallerArgumentExpression(nameof(shouldThrow))] string? expression = null) where TException : Exception
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

        throw new AssertException(expression, typeof(TException).FullName, "No exception was thrown.");
    }

    public static async Task<TException> ShouldThrowAsync<TException>(this Func<Task> shouldThrowAsync, string expectedMessage, [CallerArgumentExpression(nameof(shouldThrowAsync))] string? expression = null) where TException : Exception
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

        throw new AssertException(expression, typeof(TException).FullName, "No exception was thrown.");
    }

    public static void ShouldBeGreaterThan<T>(this T actual, T minimum, [CallerArgumentExpression(nameof(actual))] string? expression = null) where T: IComparable<T>
    {
        if (actual.CompareTo(minimum) <= 0)
            throw new AssertException(expression, $"> {minimum}", actual.ToString());
    }

    public static void ShouldBeGreaterThanOrEqualTo<T>(this T actual, T minimum, [CallerArgumentExpression(nameof(actual))] string? expression = null) where T: IComparable<T>
    {
        if (actual.CompareTo(minimum) < 0)
            throw new AssertException(expression, $">= {minimum}", actual.ToString());
    }

    public static void ShouldMatch<T>(this T actual, T expected, [CallerArgumentExpression(nameof(actual))] string? expression = null)
    {
        var actualJson = Json(actual);
        var expectedJson = Json(expected);
            
        if (actualJson != expectedJson)
            throw new AssertException(expression, expectedJson, actualJson);
    }

    public static void ShouldSatisfy<T>(this IEnumerable<T> actual, Action<T>[] itemExpectations, [CallerArgumentExpression(nameof(actual))] string? expression = null)
    {
        var actualItems = actual.ToArray();

        if (actualItems.Length != itemExpectations.Length)
            throw new AssertException(
                expression,
                $"{itemExpectations.Length} items",
                $"{actualItems.Length} items");

        for (var i = 0; i < actualItems.Length; i++)
            itemExpectations[i](actualItems[i]);
    }

    public static void ShouldBeGenericTypeParameter(this Type actual, string expectedName, [CallerArgumentExpression(nameof(actual))] string? expression = null)
    {
        actual.IsGenericParameter.ShouldBe(true);
        actual.FullName.ShouldBe(null);
        actual.Name.ShouldBe(expectedName);
    }

    public static void ShouldNotBeNull([NotNull] this object? actual, [CallerArgumentExpression(nameof(actual))] string? expression = null)
    {
        if (actual == null)
            throw new AssertException(expression, "not null", "null");
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

    static string Serialize(bool x) => x ? "true" : "false";
}