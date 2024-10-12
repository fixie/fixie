using System.Runtime.CompilerServices;

namespace Fixie.Tests;

public static class CustomAssertions
{
    public static void ItemsShouldSatisfy<T>(this IEnumerable<T> actual, Action<T>[] itemExpectations, [CallerArgumentExpression(nameof(actual))] string? expression = null)
    {
        var actualItems = actual.ToArray();

        if (actualItems.Length != itemExpectations.Length)
            throw new AssertException(
                $"{expression} should have {itemExpectations.Length} items but has {actualItems.Length} items.");

        for (var i = 0; i < actualItems.Length; i++)
            itemExpectations[i](actualItems[i]);
    }

    public static void ShouldBeGenericTypeParameter(this Type actual, string expectedName, [CallerArgumentExpression(nameof(actual))] string? expression = null)
    {
        actual.IsGenericParameter.ShouldBe(true);
        actual.FullName.ShouldBe(null);
        actual.Name.ShouldBe(expectedName);
    }
}
