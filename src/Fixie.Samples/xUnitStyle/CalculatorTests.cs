using Should;

namespace Fixie.Samples.xUnitStyle
{
    public class CalculatorTests
    {
        readonly Calculator calculator;

        public CalculatorTests()
        {
            calculator = new Calculator();
        }

        [Fact]
        public void ShouldAdd()
        {
            calculator.Add(2, 3).ShouldEqual(5);
        }

        [Fact]
        public void ShouldSubtract()
        {
            calculator.Subtract(5, 3).ShouldEqual(2);
        }
    }
}