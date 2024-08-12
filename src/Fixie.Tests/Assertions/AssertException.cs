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

    public static AssertException Create(string? expression, bool expected, bool actual)
    {
        return new AssertException(expression, Serialize(expected), Serialize(actual));
    }

    public static AssertException Create(string? expression, char expected, char actual)
    {
        return new AssertException(expression, Serialize(expected), Serialize(actual));
    }
    
    public static AssertException Create(string? expression, Type expected, Type? actual)
    {
        return new AssertException(expression, Serialize(expected), actual == null ? null : Serialize(actual));
    }

    public static AssertException Create(string? expression, string? expected, string? actual)
    {
        return new AssertException(expression, expected == null ? null : Serialize(expected), actual == null ? null : Serialize(actual));
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

    static string Serialize(bool x) => x ? "true" : "false";

    static string Serialize(char x) =>
        $"'{Escape(x)}'";

    static string Serialize(string x) =>
        $"\"{string.Join("", x.Select(Escape))}\"";
    
    static string Escape(char x) =>
        x switch
        {
            '\0' => @"\0",
            '\a' => @"\a",
            '\b' => @"\b",
            '\t' => @"\t",
            '\n' => @"\n",
            '\v' => @"\v",
            '\f' => @"\f",
            '\r' => @"\r",
            //'\e' => @"\e", TODO: Applicable in C# 13
            ' ' => " ",
            '"' => @"\""",
            '\'' => @"\'",
            '\\' => @"\\",
            _ when (char.IsControl(x) || char.IsWhiteSpace(x)) => $"\\u{(int)x:X4}",
            _ => x.ToString()
        };

    static string Serialize(Type x) =>
        $"typeof({x switch
        {
            _ when x == typeof(bool) => "bool",
            _ when x == typeof(sbyte) => "sbyte",
            _ when x == typeof(byte) => "byte",
            _ when x == typeof(short) => "short",
            _ when x == typeof(ushort) => "ushort",
            _ when x == typeof(int) => "int",
            _ when x == typeof(uint) => "uint",
            _ when x == typeof(long) => "long",
            _ when x == typeof(ulong) => "ulong",
            _ when x == typeof(nint) => "nint",
            _ when x == typeof(nuint) => "nuint",
            _ when x == typeof(decimal) => "decimal",
            _ when x == typeof(double) => "double",
            _ when x == typeof(float) => "float",
            _ when x == typeof(char) => "char",
            _ when x == typeof(string) => "string",
            _ when x == typeof(object) => "object",
            _ => x.ToString()
        }})";
}