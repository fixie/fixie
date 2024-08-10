using static System.Environment;

namespace Fixie.Tests.Assertions;

public class AssertException : Exception
{
    public static string FilterStackTraceAssemblyPrefix = typeof(AssertException).Namespace + ".";

    public string? Expression { get; }
    public string? Expected { get; }
    public string? Actual { get; }
    public bool HasCompactRepresentations { get; }

    public AssertException(string? expression, string? expected, string? actual)
    {
        Expression = expression;
        Expected = expected;
        Actual = actual;
        HasCompactRepresentations = HasCompactRepresentation(expected) &&
                                    HasCompactRepresentation(actual);
    }

    public override string Message
    {
        get
        {
            var expected = Expected ?? "null";
            var actual = Actual ?? "null";

            if (HasCompactRepresentations)
                return $"{Expression} should be {expected} but was {actual}";

            return $"{Expression} should be {NewLine}{expected}{NewLine}{NewLine}" +
                   $"but was {NewLine}{actual}";
        }
    }

    static bool HasCompactRepresentation(string? value)
    {
        const int compactLength = 50;

        if (value is null)
            return true;

        return value.Length <= compactLength && !value.Contains(NewLine);
    }

    public override string? StackTrace => FilterStackTrace(base.StackTrace);

    static string? FilterStackTrace(string? stackTrace)
    {
        if (stackTrace == null)
            return null;

        List<string> results = [];

        foreach (var line in Lines(stackTrace))
        {
            var trimmedLine = line.TrimStart();
            if (!trimmedLine.StartsWith("at " + FilterStackTraceAssemblyPrefix))
                results.Add(line);
        }

        return string.Join(NewLine, results.ToArray());
    }

    static string[] Lines(string input)
    {
        return input.Split(new[] {NewLine}, StringSplitOptions.None);
    }
}