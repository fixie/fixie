namespace Should.Core.Exceptions
{
    public class FalseException : AssertException
    {
        public FalseException(string userMessage)
            : base(userMessage ?? "Assert.False() Failure") { }
    }
}