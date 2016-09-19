using System.Collections;
using System.Collections.Generic;
using Should.Core.Assertions;

namespace Should
{
    using System;
    using Core.Exceptions;

    public static class CollectionAssertExtensions
    {
        public static void ShouldBeEmpty(this IEnumerable collection)
        {
            if (collection == null) throw new ArgumentNullException("collection", "cannot be null");

            foreach (object @object in collection)
                throw new EmptyException();
        }

        public static void ShouldContain<T>(this IEnumerable<T> collection,
                                            T expected)
        {
            var comparer = new AssertEqualityComparer<T>();

            foreach (T item in collection)
                if (comparer.Equals(expected, item))
                    return;

            throw new ContainsException(expected);
        }
    }
}