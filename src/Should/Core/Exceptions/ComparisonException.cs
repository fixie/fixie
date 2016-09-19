namespace Should.Core.Exceptions
{
    public abstract class ComparisonException : AssertException
    {
        public string Left { get; private set; }
        public string Right { get; private set; }

        protected ComparisonException(object left, object right, string methodName, string operation)
            : base($"Assert.{methodName}() Failure:\r\n\tExpected: {Format(right)} {operation} {Format(left)}\r\n\tbut it was not")
        {
            Left = left?.ToString();
            Right = right?.ToString();
        }

        protected ComparisonException(object left, object right, string message) : base(message)
        {
            Left = left?.ToString();
            Right = right?.ToString();
        }

        public static string Format(object value)
        {
            if (value == null)
            {
                return "(null)";
            }
            var type = value.GetType();
            return type == typeof(string) // || type == typeof(DateTime) || type == typeof(DateTime?)
                ? $"\"{value}\""
                : value.ToString();
        }
    }
}