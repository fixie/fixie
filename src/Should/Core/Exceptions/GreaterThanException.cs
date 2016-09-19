namespace Should.Core.Exceptions
{
    public class GreaterThanException : ComparisonException
    {
        public GreaterThanException(object left, object right) 
            : base(right, left, "GreaterThan", ">")
        { }

        public GreaterThanException(object left, object right, string message)
            : base(left, right, message)
        { }
    }
}