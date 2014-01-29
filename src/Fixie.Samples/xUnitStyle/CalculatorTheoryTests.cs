using System;
using System.Text;
using Should;

namespace Fixie.Samples.xUnitStyle
{
    using Xunit.Extensions;

    public class CalculatorTheoryTests : IDisposable
    {
        readonly Calculator calculator;
        readonly StringBuilder log;

        public CalculatorTheoryTests()
        {
            calculator = new Calculator();
            log = new StringBuilder();
            log.WhereAmI();
        }

        [Theory]
        [InlineData(2, 5, 10)]
        [InlineData(3, 4, 12)]
        [InlineData(5, 7, 35)]
        public void ShouldMultiply(int multiplier, int multiplicand, int product)
        {
            log.WhereAmI();
            calculator.Multiply(multiplier, multiplicand).ShouldEqual(product);
        }

        public void Dispose()
        {
            log.WhereAmI();

            log.ShouldHaveLines(
                ".ctor",
                "ShouldMultiply",
                "Dispose");
        }
    }
}