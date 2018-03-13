namespace Fixie.Samples.Static
{
    using Assertions;

    public static class CalculatorTests
    {
        public static void ShouldAdd() => new Calculator().Add(2, 3).ShouldEqual(5);

        public static void ShouldSubtract() => new Calculator().Subtract(5, 3).ShouldEqual(2);
    }
}