using System;
using System.Runtime.CompilerServices;
using System.Text;
using Should;

namespace Fixie.Samples.NUnitStyle
{
    [TestFixture]
    public class CalculatorTests : IDisposable
    {
        Calculator calculator;
        readonly StringBuilder log;

        public CalculatorTests()
        {
            log = new StringBuilder();
            WhereAmI();
        }

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            WhereAmI();
            calculator = new Calculator();
        }

        [SetUp]
        public void SetUp()
        {
            WhereAmI();
        }

        [Test]
        public void ShouldAdd()
        {
            WhereAmI();
            calculator.Add(2, 3).ShouldEqual(5);
        }

        [Test]
        public void ShouldSubtract()
        {
            WhereAmI();
            calculator.Subtract(5, 3).ShouldEqual(2);
        }

        [TearDown]
        public void TearDown()
        {
            WhereAmI();
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            WhereAmI();
        }

        public void Dispose()
        {
            WhereAmI();
            log.ToString().ShouldEqual(
                new StringBuilder()
                .AppendLine(".ctor")
                .AppendLine("TestFixtureSetUp")
                .AppendLine("SetUp")
                .AppendLine("ShouldAdd")
                .AppendLine("TearDown")
                .AppendLine("SetUp")
                .AppendLine("ShouldSubtract")
                .AppendLine("TearDown")
                .AppendLine("TestFixtureTearDown")
                .AppendLine("Dispose")
                .ToString());
        }

        private void Fail([CallerMemberName] string method = null)
        {
            log.AppendLine(method + " is about to throw...");
            throw new Exception(method + " Threw!");
        }

        private void WhereAmI([CallerMemberName] string method = null)
        {
            log.AppendLine(method);
        }
    }
}