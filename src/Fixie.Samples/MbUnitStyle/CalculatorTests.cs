using System;
using System.Text;
using Should;

namespace Fixie.Samples.MbUnitStyle
{
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

        [FixtureSetUp]
        public void FixtureSetUp()
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
        [Row(2, 3, 5)]
        [Row(3, 5, 8)]
        public void ShouldAdd(int a, int b, int expectedSum)
        {
            log.AppendLine($"ShouldAdd({a}, {b}, {expectedSum})");
            calculator.Add(a, b).ShouldEqual(expectedSum);
        }

        [Test]
        public void ShouldSubtract([Column(7, 8, 9)] int a,
                                   [Column(5, 6)] int b)
        {
            log.AppendLine($"ShouldSubtract({a}, {b})");
            calculator.Subtract(a, b).ShouldEqual(a - b);
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

        [FixtureTearDown]
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
                "ShouldAdd(2, 3, 5)",
                "TearDown",
                "SetUp",
                "ShouldAdd(3, 5, 8)",
                "TearDown",
                "SetUp",
                "ShouldDivide",
                "TearDown",
                "SetUp",
                "ShouldSubtract(7, 5)",
                "TearDown",
                "SetUp",
                "ShouldSubtract(7, 6)",
                "TearDown",
                "SetUp",
                "ShouldSubtract(8, 5)",
                "TearDown",
                "SetUp",
                "ShouldSubtract(8, 6)",
                "TearDown",
                "SetUp",
                "ShouldSubtract(9, 5)",
                "TearDown",
                "SetUp",
                "ShouldSubtract(9, 6)",
                "TearDown",
                "SetUp",
                "ShouldThrowWhenDividingByZero",
                "TearDown",
                "FixtureTearDown",
                "Dispose");
        }
    }
}