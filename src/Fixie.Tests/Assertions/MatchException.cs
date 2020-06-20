namespace Fixie.Tests.Assertions
{
    using static System.Environment;

    public class MatchException : AssertException
    {
        public string? Expected { get; }
        public string? Actual { get; }

        public MatchException(string? expected, string? actual)
            : base(
                $"Expected:{NewLine}{expected}{NewLine}{NewLine}" +
                $"Actual:{NewLine}{actual}")
        {
            Expected = expected;
            Actual = actual;
        }
    }
}