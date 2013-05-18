using Should;

namespace Fixie.Samples.NUnitStyle
{
    [TestFixture]
    public class CalculatorTests
    {
        readonly Calculator calculator;

        public CalculatorTests()
        {
            calculator = new Calculator();
        }

        [Test]
        public void ShouldAdd()
        {
            calculator.Add(2, 3).ShouldEqual(5);
        }

        [Test]
        public void ShouldSubtract()
        {
            calculator.Subtract(5, 3).ShouldEqual(2);
        }
    }
}