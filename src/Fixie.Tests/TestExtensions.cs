namespace Fixie.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using Fixie.Internal;

    static class TestExtensions
    {
        const BindingFlags InstanceMethods = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

        public static IEnumerable<TestMethod> OrderByName(this IEnumerable<TestMethod> tests)
            => tests.OrderBy(x => x.Method.Name, StringComparer.Ordinal);

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

        public static IEnumerable<string> Lines(this RedirectedConsole console)
        {
            return console.Output.Lines();
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

        public static IEnumerable<string> CleanStackTraceLineNumbers(this IEnumerable<string> lines)
        {
            //Avoid brittle assertion introduced by stack trace line numbers.

            return lines.Select(line => Regex.Replace(line, @":line \d+", ":line #"));
        }

        public static IEnumerable<string> CleanDuration(this IEnumerable<string> lines)
        {
            //Avoid brittle assertion introduced by test duration.

            var decimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;

            return lines.Select(line => Regex.Replace(line, @"took [\d" + Regex.Escape(decimalSeparator) + @"]+ seconds", @"took 1.23 seconds"));
        }
    }
}