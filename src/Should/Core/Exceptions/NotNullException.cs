namespace Should.Core.Exceptions
{
    public class NotNullException : AssertException
    {
        public NotNullException()
            : this("Assert.NotNull() Failure") { }

        public NotNullException(string message)
            : base(message) { }
    }
}