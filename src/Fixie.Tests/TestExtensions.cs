using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
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

    public static string NormalizeStackTraces(this string? multiline)
    {
        //Avoid brittle assertion introduced by stack trace absolute paths and line numbers.

        if (multiline == null)
            throw new Exception("Expected a non-null string.");

        return Regex.Replace(multiline,
            @"\) in .+([\\/])src([\\/])Fixie(.+)\.cs:line \d+",
            ") in ...$1src$2Fixie$3.cs:line #");
    }

    public static void ShouldBeStackTrace(this string? actual, string[] expected, [CallerArgumentExpression(nameof(actual))] string? expression = null)
    {
        actual
            .NormalizeStackTraces()
            .ShouldBe(string.Join(Environment.NewLine, expected), expression);
    }

    public static string CleanDuration(this string multiline)
    {
        //Avoid brittle assertion introduced by test duration.

        var decimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;

        return Regex.Replace(multiline, @"took [\d" + Regex.Escape(decimalSeparator) + "]+ seconds", "took 1.23 seconds");
    }
}