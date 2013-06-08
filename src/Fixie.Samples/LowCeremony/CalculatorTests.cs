using System;
using System.Runtime.CompilerServices;
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
            WhereAmI();
        }

        public void FixtureSetUp()
        {
            WhereAmI();
            calculator = new Calculator();
        }

        public void SetUp()
        {
            WhereAmI();
        }

        public void ShouldAdd()
        {
            WhereAmI();
            calculator.Add(2, 3).ShouldEqual(5);
        }

        public void ShouldSubtract()
        {
            WhereAmI();
            calculator.Subtract(5, 3).ShouldEqual(2);
        }

        public void TearDown()
        {
            WhereAmI();
        }

        public void FixtureTearDown()
        {
            WhereAmI();
        }

        public void Dispose()
        {
            WhereAmI();
            log.ToString().ShouldEqual(
                new StringBuilder()
                .AppendLine(".ctor")
                .AppendLine("FixtureSetUp")
                .AppendLine("SetUp")
                .AppendLine("ShouldAdd")
                .AppendLine("TearDown")
                .AppendLine("SetUp")
                .AppendLine("ShouldSubtract")
                .AppendLine("TearDown")
                .AppendLine("FixtureTearDown")
                .AppendLine("Dispose")
                .ToString());
        }

        private void WhereAmI([CallerMemberName] string method = null)
        {
            log.AppendLine(method);
        }
    }
}