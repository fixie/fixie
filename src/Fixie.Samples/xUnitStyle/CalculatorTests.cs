using System;
using System.Runtime.CompilerServices;
using System.Text;
using Should;

namespace Fixie.Samples.xUnitStyle
{
    public class CalculatorTests : IUseFixture<FixtureData>, IUseFixture<DisposableFixtureData>, IDisposable
    {
        readonly Calculator calculator;
        readonly StringBuilder log;

        bool executedAddTest = false;
        bool executedSubtractTest = false;

        FixtureData fixtureData;
        DisposableFixtureData disposableFixtureData;

        public CalculatorTests()
        {
            calculator = new Calculator();
            log = new StringBuilder();
            WhereAmI();
        }

        public void SetFixture(FixtureData data)
        {
            WhereAmI();
            fixtureData = data;
            log.AppendLine("   FixtureData " + fixtureData.Instance);
            data.Instance.ShouldEqual(1);
        }

        public void SetFixture(DisposableFixtureData data)
        {
            WhereAmI();
            disposableFixtureData = data;
            log.AppendLine("   DisposableFixtureData " + disposableFixtureData.Instance);
            data.Instance.ShouldEqual(1);
        }

        [Fact]
        public void ShouldAdd()
        {
            executedAddTest = true;
            WhereAmI();
            calculator.Add(2, 3).ShouldEqual(5);
        }

        [Fact]
        public void ShouldSubtract()
        {
            executedSubtractTest = true;
            WhereAmI();
            calculator.Subtract(5, 3).ShouldEqual(2);
        }

        public void Dispose()
        {
            WhereAmI();
            (executedAddTest && executedSubtractTest).ShouldBeFalse();
            (executedAddTest || executedSubtractTest).ShouldBeTrue();

            log.ToString().ShouldEqual(
                new StringBuilder()
                    .AppendLine(".ctor")
                    .AppendLine("SetFixture")
                    .AppendLine("   FixtureData 1")
                    .AppendLine("SetFixture")
                    .AppendLine("   DisposableFixtureData 1")
                    .AppendLine(executedAddTest ? "ShouldAdd": "ShouldSubtract")
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

    public class DisposableFixtureData : IDisposable
    {
        static int InstanceCounter;
        public static int DisposalCount { get; private set; }

        public int Instance { get; private set; }

        public DisposableFixtureData()
        {
            Instance = ++InstanceCounter;
        }

        public void Dispose()
        {
            DisposalCount++;
        }
    }

    public class FixtureData
    {
        static int InstanceCounter;

        public int Instance { get; private set; }

        public FixtureData()
        {
            Instance = ++InstanceCounter;
        }
    }
}