namespace Should.Core.Exceptions
{
    public class GreaterThanException : ComparisonException
    {
        public GreaterThanException(object left, object right) 
            : base(right, left, "GreaterThan", ">")
        { }
    }
}