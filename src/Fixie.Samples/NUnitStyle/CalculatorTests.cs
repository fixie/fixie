using Should;

namespace Fixie.Samples.NUnitStyle
{
    [TestFixture]
    public class CalculatorTests
    {
        Calculator calculator;

        [SetUp]
        public void SetUp()
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

        [TearDown]
        public void TearDown()
        {
        }
    }
}