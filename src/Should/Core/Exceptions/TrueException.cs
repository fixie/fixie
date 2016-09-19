namespace Should.Core.Exceptions
{
    public class TrueException : AssertException
    {
        public TrueException(string userMessage)
            : base(userMessage ?? "Assert.True() Failure") { }
    }
}