using System;
using System.Text;
using Should;

namespace Fixie.Samples.Skipped
{
    public class SkipMethodTests : IDisposable
    {
        Calculator calculator;
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