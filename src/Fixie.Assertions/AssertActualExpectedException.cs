namespace Fixie.Assertions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    using static System.Environment;

    public class AssertActualExpectedException : AssertException
    {
        public AssertActualExpectedException(object expected, object actual)
            : base(BuildMessage(expected, actual))
        {
        }

        public AssertActualExpectedException(object expected, object actual, string userMessage)
            : base(BuildMessage(expected, actual, userMessage))
        {
        }

        static string BuildMessage(object expected, object actual, string userMessage = null)
        {
            string differencePosition = null;

            if (actual is IEnumerable enumerableActual && expected is IEnumerable enumerableExpected)
            {
                var comparer = new EnumerableEqualityComparer();
                comparer.Equals(enumerableActual, enumerableExpected);

                differencePosition = "First difference is at position " + comparer.Position;
            }

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

            var message = new StringBuilder();

            if (userMessage != null)
            {
                message.AppendLine(userMessage);
                message.AppendLine();
            }

            if (differencePosition != null)
                message.AppendLine(differencePosition);

            message.AppendLine($"Expected: {FormatMultiLine(expectedStr ?? "(null)")}");
            message.Append($"Actual:   {FormatMultiLine(actualStr ?? "(null)")}");

            return message.ToString();
        }

        static string ConvertToString(object value)
        {
            if (value is Array valueArray)
            {
                var valueStrings = new List<string>();

                foreach (object valueObject in valueArray)
                    valueStrings.Add(valueObject == null ? "(null)" : valueObject.ToString());

                return value.GetType().FullName + " { " + String.Join(", ", valueStrings.ToArray()) + " }";
            }

            return value.ToString();
        }

        static string FormatMultiLine(string value)
            => value.Replace(NewLine, NewLine + "          ");
    }
}