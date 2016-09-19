namespace Should
{
    using Core.Exceptions;

    public static class BooleanAssertionExtensions
    {
        public static void ShouldBeFalse(this bool condition)
        {
            if (condition)
                throw new FalseException(null);
        }

        public static void ShouldBeFalse(this bool condition,
                                         string userMessage)
        {
            if (condition)
                throw new FalseException(userMessage);
        }

        public static void ShouldBeTrue(this bool condition)
        {
            if (!condition)
                throw new TrueException(null);
        }

        public static void ShouldBeTrue(this bool condition,
                                        string userMessage)
        {
            if (!condition)
                throw new TrueException(userMessage);
        }
    }
}