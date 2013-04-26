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

        public static void ShouldHaveEntries(this StubListener actual, params string[] expected)
        {
            //Type.GetMethods(...) makes no guarantees about the order of the items returned,
            //so test execution order is not guaranteed, although in practice it is predictable.

            actual.Entries.OrderBy(x => x)
                .ShouldEqual(expected.OrderBy(x => x).ToArray());
        }
    }
}