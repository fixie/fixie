using System.Collections;
using System.Collections.Generic;
using Should.Core.Assertions;

namespace Should
{
    public static class CollectionAssertExtensions
    {
        public static void ShouldBeEmpty(this IEnumerable collection)
        {
            Assert.Empty(collection);
        }

        public static void ShouldContain<T>(this IEnumerable<T> collection,
                                            T expected)
        {
            Assert.Contains(expected, collection);
        }
    }
}