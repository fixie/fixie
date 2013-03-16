using System.Collections.Generic;
using System.Linq;
using Should.Core.Assertions;

namespace Fixie.Tests
{
    public static class Assertions
    {
        public static void ShouldEqual<T>(this IEnumerable<T> actual, params T[] expected)
        {
            Assert.Equal(expected, actual.ToArray());
        }
    }
}