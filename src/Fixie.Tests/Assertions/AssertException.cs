using static System.Environment;

namespace Fixie.Tests.Assertions;

public class AssertException : Exception
{
    public static string FilterStackTraceAssemblyPrefix = typeof(AssertException).Namespace + ".";

    public string? Expression { get; }
    public string Expected { get; }
    public string Actual { get; }
    public bool HasMultilineRepresentation { get; }
    readonly string message;

    AssertException(string? expression, string expected, string actual, string? message = null)
    {
        HasMultilineRepresentation = IsMultiline(expected) || IsMultiline(actual);

        Expression = expression;
        Expected = expected;
        Actual = actual;

        if (message == null)
        {
            this.message = HasMultilineRepresentation
                ? MultilineMessage(Expression, Expected, Actual)
                : ScalarMessage(Expression, Expected, Actual);
        }
        else
        {
            this.message = message;
        }
    }

    public static AssertException ForValues<T>(string? expression, T expected, T actual)
    {
        return new AssertException(expression, SerializeByType(expected), SerializeByType(actual));
    }

    public static AssertException ForDescriptions(string? expression, string? expectationDescription, string? actualDescription)
    {
        return new AssertException(expression, expectationDescription ?? "null", actualDescription ?? "null");
    }

    public static AssertException ForLists<T>(string? expression, T[] expected, T[] actual)
    {
        return new AssertException(expression, SerializeList(expected), SerializeList(actual));
    }
    
    public static AssertException ForMessage(string? expression, string expected, string actual, string message)
    {
        return new AssertException(expression, expected, actual, message);
    }

    public override string Message => message;

    static string MultilineMessage(string? expression, string expected, string actual)
    {
        return $"{expression} should be{NewLine}{Indent(expected)}{NewLine}{NewLine}" +
               $"but was{NewLine}{Indent(actual)}";
    }

    static string ScalarMessage(string? expression, string expected, string actual)
    {
        return $"{expression} should be {expected} but was {actual}";
    }

    static bool IsMultiline(string? value)
    {
        if (value == null)
            return false;

        var lines = value.Split(NewLine);

        return lines.Length > 1 && lines.All(line => !line.Contains("\r") && !line.Contains("\n"));
    }

    static string Indent(string multiline) =>
        string.Join(NewLine, multiline.Split(NewLine).Select(x => $"    {x}"));

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

    static string Serialize(char x) => $"'{Escape(x)}'";

    static string Serialize(object? x) => x?.ToString() ?? "null";
    
    static string Serialize(string? x)
    {
        if (x == null)
            return "null";

        if (IsMultiline(x))
        {
            var terminal = RawStringTerminal(x);

            return $"{terminal}{NewLine}{x}{NewLine}{terminal}";
        }
        
        return $"\"{string.Join("", x.Select(Escape))}\"";
    }

    static string RawStringTerminal(string x)
    {
        var longestDoubleQuoteSequence = 0;
        var currentDoubleQuoteSequence = 0;

        foreach (var c in x)
        {
            if (c != '\"')
            {
                currentDoubleQuoteSequence = 0;
                continue;
            }

            currentDoubleQuoteSequence++;
            if (currentDoubleQuoteSequence > longestDoubleQuoteSequence)
                longestDoubleQuoteSequence = currentDoubleQuoteSequence;
        }

        return new string('\"', longestDoubleQuoteSequence < 3 ? 3 : longestDoubleQuoteSequence + 1);
    }

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

    static string Serialize(Type? x) =>
        x == null
            ? "null"
            : $"typeof({x switch
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

    static string SerializeList<T>(T[] items)
    {
        var formattedItems = string.Join("," + NewLine, items.Select(arg => "    " + SerializeByType(arg)));

        return $"[{NewLine}{formattedItems}{NewLine}]";
    }

    static string SerializeByType<T>(T any)
    {
        if (any == null) return "null";

        if (typeof(T) == typeof(bool))
            return Serialize((bool)(object)any);
        
        if (typeof(T) == typeof(char))
            return Serialize((char)(object)any);
        
        if (typeof(T) == typeof(string))
            return Serialize((string)(object)any);

        if (typeof(T) == typeof(Type))
            return Serialize((Type)(object)any);

        return Serialize(any);
    }
}