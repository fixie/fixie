using System;
using System.Text;
using Should;

namespace Fixie.Samples.LowCeremony
{
    public class CalculatorTests : IDisposable
    {
        Calculator calculator;
        readonly StringBuilder log;

        public CalculatorTests()
        {
            log = new StringBuilder();
            log.WhereAmI();
        }

        public void FixtureSetUp()
        {
            log.WhereAmI();
            calculator = new Calculator();
        }

        public void SetUp()
        {
            log.WhereAmI();
        }

        public void ShouldAdd()
        {
            log.WhereAmI();
            calculator.Add(2, 3).ShouldEqual(5);
        }

        public void ShouldSubtract()
        {
            log.WhereAmI();
            calculator.Subtract(5, 3).ShouldEqual(2);
        }

        public void TearDown()
        {
            log.WhereAmI();
        }

        public void FixtureTearDown()
        {
            log.WhereAmI();
        }

        public void Dispose()
        {
            log.WhereAmI();
            log.ShouldHaveLines(
                ".ctor",
                "FixtureSetUp",
                "SetUp",
                "ShouldAdd",
                "TearDown",
                "SetUp",
                "ShouldSubtract",
                "TearDown",
                "FixtureTearDown",
                "Dispose");
        }
    }
}