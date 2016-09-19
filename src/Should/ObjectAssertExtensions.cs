using Should.Core.Assertions;

namespace Should
{
    public static class ObjectAssertExtensions
    {
        public static void ShouldBeGreaterThan<T>(this T @object, T value)
        {
            Assert.GreaterThan(@object, value);
        }

        public static void ShouldBeGreaterThanOrEqualTo<T>(this T @object, T value)
        {
            Assert.GreaterThanOrEqual(@object, value);
        }

        public static void ShouldBeNull(this object @object)
        {
            Assert.Null(@object);
        }

        public static T ShouldBeType<T>(this object @object)
        {
            return Assert.IsType<T>(@object);
        }

        public static void ShouldEqual<T>(this T actual,
                                          T expected)
        {
            Assert.Equal(expected, actual);
        }

        public static void ShouldEqual<T>(this T actual,
                                          T expected,
                                          string userMessage)
        {
            Assert.Equal(expected, actual, userMessage);
        }

        public static T ShouldNotBeNull<T>(this T @object) where T : class
        {
            Assert.NotNull(@object);
            return @object;
        }

        public static void ShouldNotEqual<T>(this T actual,
                                             T expected)
        {
            Assert.NotEqual(expected, actual);
        }
    }
}