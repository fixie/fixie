namespace Fixie.Assertions.Core.Exceptions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Assertions;

    public class AssertActualExpectedException : AssertException
    {
        readonly string expected;
        readonly string actual;
        readonly string differencePosition = "";

        public AssertActualExpectedException(object expected, object actual)
            : this(expected, actual, "Assertion Failure")
        {
        }

        public AssertActualExpectedException(object expected,
                                             object actual,
                                             string userMessage)
            : base(userMessage)
        {
            var enumerableActual = actual as IEnumerable;
            var enumerableExpected = expected as IEnumerable;

            if (enumerableActual != null && enumerableExpected != null)
            {
                var comparer = new EnumerableEqualityComparer();
                comparer.Equals(enumerableActual, enumerableExpected);

                differencePosition = "Position: First difference is at position " + comparer.Position + Environment.NewLine;
            }

            this.actual = actual == null ? null : ConvertToString(actual);
            this.expected = expected == null ? null : ConvertToString(expected);

            if (actual != null &&
                expected != null &&
                actual.ToString() == expected.ToString() &&
                actual.GetType() != expected.GetType())
            {
                this.actual += $" ({actual.GetType().FullName})";
                this.expected += $" ({expected.GetType().FullName})";
            }
        }

        public override string Message
        {
            get
            {
                return string.Format("{0}{4}{1}Expected: {2}{4}Actual:   {3}",
                                     base.Message,
                                     differencePosition,
                                     FormatMultiLine(expected ?? "(null)"),
                                     FormatMultiLine(actual ?? "(null)"),
                                     Environment.NewLine);
            }
        }

        static string ConvertToString(object value)
        {
            var valueArray = value as Array;
            if (valueArray == null)
                return value.ToString();

            var valueStrings = new List<string>();

            foreach (object valueObject in valueArray)
                valueStrings.Add(valueObject == null ? "(null)" : valueObject.ToString());

            return value.GetType().FullName + " { " + String.Join(", ", valueStrings.ToArray()) + " }";
        }

        static string FormatMultiLine(string value)
        {
            return value.Replace(Environment.NewLine, Environment.NewLine + "          ");
        }
    }
}