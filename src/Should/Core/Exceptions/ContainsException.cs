namespace Should.Core.Exceptions
{
    public class ContainsException : AssertException
    {
        public ContainsException(object expected)
            : base(string.Format("Assert.Contains() failure: Not found: {0}", expected)) { }
    }
}