using System;
using System.Text;
using Should;

namespace Fixie.Samples.ColumnParameter
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

        public void ShouldAdd([Column(1, 2)] int a,
                              [Column(3, 4, 5)] int b)
        {
            log.AppendFormat("ShouldAdd({0}, {1})", a, b);
            log.AppendLine();
            calculator.Add(a, b).ShouldEqual(a + b);
        }

        public void ShouldSubtract([Column(7, 8, 9)] int a,
                                   [Column(5, 6)] int b)
        {
            log.AppendFormat("ShouldSubtract({0}, {1})", a, b);
            log.AppendLine();
            calculator.Subtract(a, b).ShouldEqual(a - b);
        }

        public void Dispose()
        {
            log.WhereAmI();
            log.ShouldHaveLines(
                ".ctor",
                "ShouldAdd(1, 3)",
                "ShouldAdd(1, 4)",
                "ShouldAdd(1, 5)",
                "ShouldAdd(2, 3)",
                "ShouldAdd(2, 4)",
                "ShouldAdd(2, 5)",
                "ShouldSubtract(7, 5)",
                "ShouldSubtract(7, 6)",
                "ShouldSubtract(8, 5)",
                "ShouldSubtract(8, 6)",
                "ShouldSubtract(9, 5)",
                "ShouldSubtract(9, 6)",
                "Dispose");
        }
    }
}
