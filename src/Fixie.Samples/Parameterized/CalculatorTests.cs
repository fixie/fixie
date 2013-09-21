using System;
using System.Text;
using Should;

namespace Fixie.Samples.Parameterized
{
    public class CalculatorTests : IDisposable
    {
        readonly Calculator calculator;
        readonly StringBuilder log;

        public CalculatorTests()
        {
            calculator = new Calculator();
            log = new StringBuilder();
            log.WhereAmI();
        }

        [Input(2, 3, 5)]
        [Input(3, 5, 8)]
        public void ShouldAdd(int a, int b, int expectedSum)
        {
            log.AppendFormat("ShouldAdd({0}, {1}, {2})", a, b, expectedSum);
            log.AppendLine();
            calculator.Add(a, b).ShouldEqual(expectedSum);
        }

        [Input(5, 3, 2)]
        [Input(8, 5, 3)]
        [Input(10, 5, 5)]
        public void ShouldSubtract(int a, int b, int expectedDifference)
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
                "ShouldAdd(2, 3, 5)",
                "ShouldAdd(3, 5, 8)",
                "ShouldSubtract(5, 3, 2)",
                "ShouldSubtract(8, 5, 3)",
                "ShouldSubtract(10, 5, 5)",
                "Dispose");
        }
    }
}
