namespace Fixie.Samples.Categories
{
    using Assertions;

    public class CalculatorTests
    {
        readonly Calculator calculator;

        public CalculatorTests()
        {
            calculator = new Calculator();
        }

        [CategoryA]
        public void ShouldAdd()
        {
            calculator.Add(2, 3).ShouldEqual(5);
        }

        [CategoryB]
        public void ShouldSubtract()
        {
            calculator.Add(2, 3).ShouldEqual(5);
        }
    }
}