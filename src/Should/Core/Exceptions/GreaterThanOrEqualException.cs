namespace Should.Core.Exceptions
{
    public class GreaterThanOrEqualException : ComparisonException
    {
        public GreaterThanOrEqualException(object left, object right) 
            : base(right, left, "GreaterThanOrEqual", ">=")
        { }

        public GreaterThanOrEqualException(object left, object right, string message)
            : base(left, right, message) 
        { }
    }
}