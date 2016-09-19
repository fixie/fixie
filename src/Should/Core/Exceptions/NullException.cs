namespace Should.Core.Exceptions
{
    public class NullException : AssertActualExpectedException
    {
        public NullException(object actual)
            : base(null, actual, "Assert.Null() Failure") { }
    }
}