using static System.Environment;

namespace Fixie.Tests.Assertions;

public class AssertException : Exception
{
    public static string FilterStackTraceAssemblyPrefix = typeof(AssertException).Namespace + ".";

    public string? Expected { get; }
    public string? Actual { get; }
    public bool HasCompactRepresentations { get; }

    public AssertException(string? expected, string? actual)
    {
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
                return $"Expected: {expected}{NewLine}" +
                       $"Actual:   {actual}";

            return $"Expected:{NewLine}{expected}{NewLine}{NewLine}" +
                   $"Actual:{NewLine}{actual}";
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

        var results = new List<string>();

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