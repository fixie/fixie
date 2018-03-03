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
        public void ShouldBeSkipped()
        {
            throw new Exception(nameof(ShouldBeSkipped) + " was invoked explicitly.");
        }

        public void ShouldSubtract()
        {
            log.WhereAmI();
            calculator.Subtract(5, 3).ShouldEqual(2);
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