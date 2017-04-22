namespace Fixie.Samples.Skipped
{
    using System;
    using System.Text;
    using Assertions;

    public class SkipMethodTests : IDisposable
    {
        readonly Calculator calculator;
        readonly StringBuilder log;

        public SkipMethodTests()
        {
            calculator = new Calculator();
            log = new StringBuilder();
            log.WhereAmI();
        }

        public void ShouldAdd()
        {
            log.WhereAmI();
            calculator.Add(2, 3).ShouldEqual(5);
        }

        [Skip]
        public void ShouldNotBeCalled()
        {
            throw new Exception("This test should be skipped.");
        }

        public void ShouldSubtract()
        {
            log.WhereAmI();
            calculator.Subtract(5, 3).ShouldEqual(2);
        }

        [Explicit]
        public void ExplicitTest()
        {
            throw new Exception("ExplicitTest was invoked explicitly.");
        }

        public void Dispose()
        {
            log.WhereAmI();
            log.ShouldHaveLines(
                ".ctor",
                "ShouldAdd",
                "ShouldSubtract",
                "Dispose");
        }
    }
}