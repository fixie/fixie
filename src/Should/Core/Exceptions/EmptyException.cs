namespace Should.Core.Exceptions
{
    public class EmptyException : AssertException
    {
        public EmptyException()
            : base("Assert.Empty() failure") { }
    }
}