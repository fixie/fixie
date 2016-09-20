namespace Fixie.Assertions.Core.Assertions
{
    using Exceptions;

    public class Assert
    {
        public static void Equal<T>(T expected, T actual)
        {
            var comparer = new AssertEqualityComparer<T>();
            if (!comparer.Equals(expected, actual))
                throw new AssertActualExpectedException(expected, actual);
        }
    }
}