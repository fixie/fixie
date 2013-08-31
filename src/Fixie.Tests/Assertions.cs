using System;
using System.Collections.Generic;
using System.Linq;
using Should;
using Should.Core.Assertions;

namespace Fixie.Tests
{
    public static class Assertions
    {
        public static void ShouldEqual<T>(this IEnumerable<T> actual, params T[] expected)
        {
            Assert.Equal(expected, actual.ToArray());
        }

        public static void ShouldThrow<TException>(this Action shouldThrow, string expectedMessage) where TException : Exception
        {
            bool threw = false;

            try
            {
                shouldThrow();
            }
            catch (Exception actual)
            {
                threw = true;
                actual.ShouldBeType<TException>();
                actual.Message.ShouldEqual(expectedMessage);
            }

            threw.ShouldBeTrue();
        }
    }
}