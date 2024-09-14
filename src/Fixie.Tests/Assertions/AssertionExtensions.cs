using System.Runtime.CompilerServices;

namespace Fixie.Tests.Assertions;

public static class AssertionExtensions
{
    public static void ShouldBe(this string? actual, string? expected, [CallerArgumentExpression(nameof(actual))] string? expression = null)
    {
        if (actual != expected)
            throw AssertException.ForValues(expression, expected, actual);
    }

    public static void ShouldBe(this bool actual, bool expected, [CallerArgumentExpression(nameof(actual))] string? expression = null)
    {
        if (actual != expected)
            throw AssertException.ForValues(expression, expected, actual);
    }
    
    public static void ShouldBe<T>(this IEquatable<T> actual, IEquatable<T> expected, [CallerArgumentExpression(nameof(actual))] string? expression = null)
    {
        if (!actual.Equals(expected))
            throw AssertException.ForValues(expression, expected, actual);
    }

    public static void ShouldBe(this Type actual, Type expected, [CallerArgumentExpression(nameof(actual))] string? expression = null)
    {
        if (actual != expected)
            throw AssertException.ForValues(expression, expected, actual);
    }

    public static void ShouldBe(this object? actual, object? expected, [CallerArgumentExpression(nameof(actual))] string? expression = null)
    {
        if (!Equals(actual, expected))
            throw AssertException.ForValues(expression, expected, actual);
    }

    public static void ShouldMatch<T>(this IEnumerable<T> actual, T[] expected, [CallerArgumentExpression(nameof(actual))] string? expression = null)
    {
        var actualArray = actual.ToArray();

        if (actualArray.Length != expected.Length)
            throw AssertException.ForLists(expression, expected, actualArray);

        foreach (var (actualItem, expectedItem) in actualArray.Zip(expected))
            if (!Equals(actualItem, expectedItem))
                throw AssertException.ForLists(expression, expected, actualArray);
    }
}