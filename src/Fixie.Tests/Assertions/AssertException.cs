namespace Fixie.Tests.Assertions
{
    using System;
    using System.Collections.Generic;
    using static System.Environment;

    public class AssertException : Exception
    {
        public static string FilterStackTraceAssemblyPrefix = typeof(AssertException).Namespace + ".";

        public string? Expected { get; }
        public string? Actual { get; }

        public AssertException(string? expected, string? actual)
            : base(ExpectationString(expected, actual))
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
}