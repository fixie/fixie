using System.Collections.Generic;
using Shouldly;

namespace Fixie.Tests
{
    public static class Assertions
    {
        public static void ShouldBe<T>(this IEnumerable<T> actual, params T[] expected)
        {
            actual.ShouldBe((IEnumerable<T>)expected);
        }
    }
}