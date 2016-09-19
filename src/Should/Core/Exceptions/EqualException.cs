namespace Should.Core.Exceptions
{
    public class EqualException : AssertActualExpectedException
    {
        public EqualException(object expected,
                              object actual)
            : this(expected, actual, "Assert.Equal() Failure") { }

        public EqualException(object expected, object actual, string userMessage)
            : base(expected, actual, userMessage) { }
    }
}