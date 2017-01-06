namespace Fixie.Samples.NUnitStyle
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Assertions;

    [TestFixture]
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

        public static IEnumerable<object[]> FieldSource = new List<object[]>
        {
            new object[] { "Internal Field", 1, 2, 3 },
            new object[] { "Internal Field", 2, 3, 5 }
        };

        public static IEnumerable<object[]> MethodSource()
        {
            return new List<object[]>
            {
                new object[] { "Internal Method", 3, 4, 7 },
                new object[] { "Internal Method", 4, 5, 9 }
            };
        }

        public static IEnumerable<object[]> PropertySource
        {
            get
            {
                return new List<object[]>
                {
                    new object[] { "Internal Property", 5, 6, 11 },
                    new object[] { "Internal Property", 6, 7, 13 }
                };
            }
        }

        [Test]
        [TestCaseSource("FieldSource")]
        [TestCaseSource("FieldSource", typeof(ExternalSourceOfTestCaseData))]
        [TestCaseSource("MethodSource")]
        [TestCaseSource("MethodSource", typeof(ExternalSourceOfTestCaseData))]
        [TestCaseSource("PropertySource")]
        [TestCaseSource("PropertySource", typeof(ExternalSourceOfTestCaseData))]
        public void ShouldAddFromFieldSource(string source, int a, int b, int expectedSum)
        {
            log.AppendFormat("{0}: ShouldAdd({1}, {2}, {3})", source, a, b, expectedSum);
            log.AppendLine();
            calculator.Add(a, b).ShouldEqual(expectedSum);
        }

        public void Dispose()
        {
            log.WhereAmI();
            log.ShouldHaveLines(
                ".ctor",
                "External Field: ShouldAdd(10, 20, 30)",
                "External Field: ShouldAdd(20, 30, 50)",
                "External Method: ShouldAdd(30, 40, 70)",
                "External Method: ShouldAdd(40, 50, 90)",
                "External Property: ShouldAdd(50, 60, 110)",
                "External Property: ShouldAdd(60, 70, 130)",
                "Internal Field: ShouldAdd(1, 2, 3)",
                "Internal Field: ShouldAdd(2, 3, 5)",
                "Internal Method: ShouldAdd(3, 4, 7)",
                "Internal Method: ShouldAdd(4, 5, 9)",
                "Internal Property: ShouldAdd(5, 6, 11)",
                "Internal Property: ShouldAdd(6, 7, 13)",
                "Dispose");
        }
    }

    public class ExternalSourceOfTestCaseData
    {
        public static IEnumerable<object[]> FieldSource = new List<object[]>
        {
            new object[] { "External Field", 10, 20, 30 },
            new object[] { "External Field", 20, 30, 50 }
        };

        public static IEnumerable<object[]> MethodSource()
        {
            return new List<object[]>
            {
                new object[] { "External Method", 30, 40, 70 },
                new object[] { "External Method", 40, 50, 90 }
            };
        }

        public static IEnumerable<object[]> PropertySource
        {
            get
            {
                return new List<object[]>
                {
                    new object[] { "External Property", 50, 60, 110 },
                    new object[] { "External Property", 60, 70, 130 }
                };
            }
        }
    }
}
