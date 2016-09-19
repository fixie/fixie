using Should.Core.Assertions;

namespace Should
{
    public static class BooleanAssertionExtensions
    {
        public static void ShouldBeFalse(this bool condition)
        {
            Assert.False(condition);
        }

        public static void ShouldBeFalse(this bool condition,
                                         string userMessage)
        {
            Assert.False(condition, userMessage);
        }

        public static void ShouldBeTrue(this bool condition)
        {
            Assert.True(condition);
        }

        public static void ShouldBeTrue(this bool condition,
                                        string userMessage)
        {
            Assert.True(condition, userMessage);
        }
    }
}