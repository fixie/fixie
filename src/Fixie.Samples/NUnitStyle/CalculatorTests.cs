namespace Fixie.Samples.NUnitStyle
{
    using System;
    using System.Text;
    using Should;

    [TestFixture]
    public class CalculatorTests : IDisposable
    {
        Calculator calculator;
        readonly StringBuilder log;

        public CalculatorTests()
        {
            log = new StringBuilder();
            log.WhereAmI();
        }

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            log.WhereAmI();
            calculator = new Calculator();
        }

        [SetUp]
        public void SetUp()
        {
            log.WhereAmI();
        }

        [Test]
        public void ShouldAdd()
        {
            log.WhereAmI();
            calculator.Add(2, 3).ShouldEqual(5);
        }

        [Test]
        public void ShouldSubtract()
        {
            log.WhereAmI();
            calculator.Subtract(5, 3).ShouldEqual(2);
        }

        [Test]
        public void ShouldDivide()
        {
            log.WhereAmI();
            calculator.Divide(6, 3).ShouldEqual(2);
        }

        [Test]
        [ExpectedException(typeof(DivideByZeroException))]
        public void ShouldThrowWhenDividingByZero()
        {
            log.WhereAmI();
            calculator.Divide(1, 0);
        }

        [TearDown]
        public void TearDown()
        {
            log.WhereAmI();
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            log.WhereAmI();
        }

        public void Dispose()
        {
            log.WhereAmI();
            log.ShouldHaveLines(
                ".ctor",
                "TestFixtureSetUp",
                "SetUp",
                "ShouldAdd",
                "TearDown",
                "SetUp",
                "ShouldDivide",
                "TearDown",
                "SetUp",
                "ShouldSubtract",
                "TearDown",
                "SetUp",
                "ShouldThrowWhenDividingByZero",
                "TearDown",
                "TestFixtureTearDown",
                "Dispose");
        }
    }
}