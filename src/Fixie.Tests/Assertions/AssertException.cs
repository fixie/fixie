namespace Fixie.Tests.Assertions
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using static System.Environment;

    public class AssertException : Exception
    {
        public static string FilterStackTraceAssemblyPrefix = typeof(AssertException).Namespace + ".";

        public AssertException(string message)
            : base(message)
        {
        }

        public AssertException(object? expected, object? actual)
            : base(ExpectationMessage(expected, actual))
        {
        }

        static string ExpectationMessage(object? expected, object? actual)
        {
            var message = new StringBuilder();

            var actualStr = actual == null ? null : ConvertToString(actual);
            var expectedStr = expected == null ? null : ConvertToString(expected);

            if (actual != null &&
                expected != null &&
                actual.ToString() == expected.ToString() &&
                actual.GetType() != expected.GetType())
            {
                actualStr += $" ({actual.GetType().FullName})";
                expectedStr += $" ({expected.GetType().FullName})";
            }

            message.AppendLine($"Expected: {FormatMultiLine(expectedStr ?? "(null)")}");
            message.Append($"Actual:   {FormatMultiLine(actualStr ?? "(null)")}");

            return message.ToString();
        }

        static string? ConvertToString(object value)
        {
            if (value is Array valueArray)
            {
                var valueStrings = new List<string>();

                foreach (var valueObject in valueArray)
                    valueStrings.Add(valueObject?.ToString() ?? "(null)");

                return value.GetType().FullName +
                       $" {{{NewLine}{string.Join("," + NewLine, valueStrings.ToArray())}{NewLine}}}";
            }

            return value.ToString();
        }

        static string FormatMultiLine(string value)
            => value.Replace(NewLine, NewLine + "          ");

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