namespace Fixie.Tests.Assertions
{
    using static System.Environment;

    public class MatchException : AssertException
    {
        public string? Expected { get; }
        public string? Actual { get; }

        public MatchException(string? expected, string? actual)
            : base(
                ExpectationString(expected, actual))
        {
            Expected = expected ?? "null";
            Actual = actual ?? "null";;
        }

        static string ExpectationString(string? expected, string? actual)
        {
            expected ??= "null";
            actual ??= "null";

            if (HasCompactRepresentation(expected) && HasCompactRepresentation(actual))
                return $"Expected: {expected}{NewLine}" +
                       $"Actual:   {actual}";

            return $"Expected:{NewLine}{expected}{NewLine}{NewLine}" +
                   $"Actual:{NewLine}{actual}";
        }

        static bool HasCompactRepresentation(string value)
        {
            const int compactLength = 50;

            return value.Length <= compactLength && !value.Contains(NewLine);
        }
    }
}