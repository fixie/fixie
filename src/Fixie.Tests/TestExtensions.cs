using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Fixie.Assertions;
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

    public static string NormalizeLineNumbers(this string multiline)
    {
        return Regex.Replace(multiline, @"\.cs:line \d+", ".cs:line #");
    }

    public static void ShouldBeStackTrace(this string? actual, string[] expected, [CallerArgumentExpression(nameof(actual))] string? expression = null)
    {
        actual.ShouldNotBeNull();
        actual
            .NormalizeLineNumbers()
            .ShouldBe(string.Join(Environment.NewLine, expected), expression);
    }

    public static string NormalizeDuration(this string multiline)
    {
        //Avoid brittle assertion introduced by test duration.

        var decimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;

        return Regex.Replace(multiline, @"took [\d" + Regex.Escape(decimalSeparator) + "]+ seconds", "took 1.23 seconds");
    }
}