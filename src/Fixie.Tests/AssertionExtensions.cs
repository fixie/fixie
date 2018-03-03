namespace Fixie.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Assertions;

    public static class AssertionExtensions
    {
        public static void ShouldEqual<T>(this IEnumerable<T> actual, params T[] expected)
        {
            Assert.Equal(expected, actual.ToArray());
        }

        public static TException ShouldThrow<TException>(this Action shouldThrow, string expectedMessage) where TException : Exception
        {
            bool threw = false;
            Exception exception = null;

            try
            {
                shouldThrow();
            }
            catch (Exception actual)
            {
                threw = true;
                actual.ShouldBeType<TException>();
                actual.Message.ShouldEqual(expectedMessage);
                exception = actual;
            }

            threw.ShouldBeTrue();
            return (TException)exception;
        }
    }
}