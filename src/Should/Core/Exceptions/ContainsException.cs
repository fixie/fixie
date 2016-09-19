namespace Should.Core.Exceptions
{
    public class ContainsException : AssertException
    {
        public ContainsException(object expected)
            : base($"Assert.Contains() failure: Not found: {expected}") { }
    }
}