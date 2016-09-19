namespace Should.Core.Exceptions
{
    public class ComparisonException : AssertException
    {
        public ComparisonException(object left, object right, string operation)
            : base($"Assertion Failure:\r\n\tExpected: {Format(left)} {operation} {Format(right)}\r\n\tbut it was not")
        {
        }

        public static string Format(object value)
        {
            return value?.ToString() ?? "(null)";
        }
    }
}