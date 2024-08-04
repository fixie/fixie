using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Fixie.Tests;

static class TestExtensions
{
    const BindingFlags InstanceMethods = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

    public static MethodInfo GetInstanceMethod(this Type type, string methodName)
    {
        var instanceMethod = type.GetMethod(methodName, InstanceMethods);

        if (instanceMethod == null)
            throw new Exception($"Could not find instance method '{methodName}' on type '{type.FullName}'.");

        return instanceMethod;
    }

    public static IReadOnlyList<MethodInfo> GetInstanceMethods(this Type type)
    {
        return type.GetMethods(InstanceMethods);
    }

    public static IEnumerable<string> Lines(this string? multiline)
    {
        if (multiline == null)
            throw new Exception("Expected a non-null string.");

        var lines = multiline.Split(new[] { Environment.NewLine }, StringSplitOptions.None).ToList();

        while (lines.Count > 0 && lines[lines.Count-1] == "")
            lines.RemoveAt(lines.Count-1);

        return lines;
    }

    public static IEnumerable<string> NormalizeStackTraceLines(this IEnumerable<string> lines)
    {
        //Avoid brittle assertion introduced by stack trace absolute paths, line numbers,
        //and platform dependent variations in the rethrow marker.

        return lines.Select(line =>
        {
            if (line == "--- End of stack trace from previous location ---")
                line = "--- End of stack trace from previous location where exception was thrown ---";

            return Regex.Replace(line,
                @"\) in .+([\\/])src([\\/])Fixie(.+)\.cs:line \d+",
                ") in ...$1src$2Fixie$3.cs:line #");
        });
    }

    public static IEnumerable<string> CleanDuration(this IEnumerable<string> lines)
    {
        //Avoid brittle assertion introduced by test duration.

        var decimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;

        return lines.Select(line => Regex.Replace(line, @"took [\d" + Regex.Escape(decimalSeparator) + @"]+ seconds", @"took 1.23 seconds"));
    }
}