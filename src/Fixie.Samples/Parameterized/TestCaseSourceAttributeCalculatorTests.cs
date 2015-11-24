using System;
using System.Text;
using Should;

namespace Fixie.Samples.Parameterized
{
    using System.Collections.Generic;

    public class TestCaseSourceAttributeCalculatorTests : IDisposable
    {
        readonly Calculator calculator;
        readonly StringBuilder log;

        public TestCaseSourceAttributeCalculatorTests()
        {
            calculator = new Calculator();
            log = new StringBuilder();
            log.WhereAmI();
        }

        public static IEnumerable<object[]> CustomFieldSource = new List<object[]>
        {
            new object[] { 10, 30, 40 },
            new object[] { 2, 14, 16 }
        };

        public static IEnumerable<object[]> CustomMethodSource()
        {
            return new List<object[]>
            {
                new object[]{1, 3, 4},
                new object[]{12, 15, 27}
            };
        }

        public static IEnumerable<object[]> CustomPropertySource => new List<object[]>
        {
            new object[] { 8, 5, 3 },
            new object[] { 5, 3, 2 },
            new object[] { 10, 5, 5 }
        };

        [TestCaseSource("CustomFieldSource")]
        public void ShouldAddFromFieldSource(int a, int b, int expectedSum)
        {
            log.AppendFormat("ShouldAdd({0}, {1}, {2})", a, b, expectedSum);
            log.AppendLine();
            calculator.Add(a, b).ShouldEqual(expectedSum);
        }

        [TestCaseSource("CustomFieldSource", typeof(ExternalSourceOfTestCaseData))]
        public void ShouldAddFromExternalFieldSource(int a, int b, int expectedSum)
        {
            log.AppendFormat("ShouldAdd({0}, {1}, {2})", a, b, expectedSum);
            log.AppendLine();
            calculator.Add(a, b).ShouldEqual(expectedSum);
        }

        [TestCaseSource("CustomMethodSource")]
        public void ShouldAddFromMethodSource(int a, int b, int expectedSum)
        {
            log.AppendFormat("ShouldAdd({0}, {1}, {2})", a, b, expectedSum);
            log.AppendLine();
            calculator.Add(a, b).ShouldEqual(expectedSum);
        }

        [TestCaseSource("CustomMethodSource", typeof(ExternalSourceOfTestCaseData))]
        public void ShouldAddFromExternalMethodSource(int a, int b, int expectedSum)
        {
            log.AppendFormat("ShouldAdd({0}, {1}, {2})", a, b, expectedSum);
            log.AppendLine();
            calculator.Add(a, b).ShouldEqual(expectedSum);
        }

        [TestCaseSource("CustomPropertySource")]
        public void ShouldSubtractFromPropertySource(int a, int b, int expectedDifference)
        {
            log.AppendFormat("ShouldSubtract({0}, {1}, {2})", a, b, expectedDifference);
            log.AppendLine();
            calculator.Subtract(a, b).ShouldEqual(expectedDifference);
        }

        [TestCaseSource("CustomPropertySource", typeof(ExternalSourceOfTestCaseData))]
        public void ShouldSubtractFromExternalPropertySource(int a, int b, int expectedDifference)
        {
            log.AppendFormat("ShouldSubtract({0}, {1}, {2})", a, b, expectedDifference);
            log.AppendLine();
            calculator.Subtract(a, b).ShouldEqual(expectedDifference);
        }

        public void Dispose()
        {
            log.WhereAmI();
            log.ShouldHaveLines(
                ".ctor",
                "ShouldAdd(100, 300, 400)",
                "ShouldAdd(22, 34, 56)",
                "ShouldAdd(11, 33, 44)",
                "ShouldAdd(110, 115, 225)",
                "ShouldAdd(10, 30, 40)",
                "ShouldAdd(2, 14, 16)",
                "ShouldAdd(1, 3, 4)",
                "ShouldAdd(12, 15, 27)",
                "ShouldSubtract(100, 35, 65)",
                "ShouldSubtract(18, 5, 13)",
                "ShouldSubtract(25, 23, 2)",
                "ShouldSubtract(10, 5, 5)",
                "ShouldSubtract(5, 3, 2)",
                "ShouldSubtract(8, 5, 3)",
                "Dispose");
        }
    }

    public class ExternalSourceOfTestCaseData
    {
        public static IEnumerable<object[]> CustomFieldSource = new List<object[]>
        {
            new object[] { 100, 300, 400 },
            new object[] { 22, 34, 56 }
        };

        public static IEnumerable<object[]> CustomMethodSource()
        {
            return new List<object[]>
            {
                new object[]{11, 33, 44},
                new object[]{110, 115, 225}
            };
        }
        public static IEnumerable<object[]> CustomPropertySource => new List<object[]>
        {
            new object[] { 18, 5, 13 },
            new object[] { 25, 23, 2 },
            new object[] { 100, 35, 65 }
        };
    }
}
