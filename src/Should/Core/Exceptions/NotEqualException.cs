namespace Should.Core.Exceptions
{
    public class NotEqualException : AssertActualExpectedException
    {
        public NotEqualException(object expected,
                              object actual)
            : this(expected, actual, "Assert.NotEqual() Failure") { }

        public NotEqualException(object expected, object actual, string userMessage)
            : base(expected, actual, userMessage) { }
    }
}