namespace Should.Core.Exceptions
{
    public class GreaterThanOrEqualException : ComparisonException
    {
        public GreaterThanOrEqualException(object left, object right) 
            : base(right, left, "GreaterThanOrEqual", ">=")
        { }
    }
}