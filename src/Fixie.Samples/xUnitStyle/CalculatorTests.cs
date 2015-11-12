using System;
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
            log.WhereAmI();
        }

        public void SetFixture(FixtureData data)
        {
            log.WhereAmI();
            fixtureData = data;
            log.AppendLine("   FixtureData " + fixtureData.Instance);
            data.Instance.ShouldEqual(1);
        }

        public void SetFixture(DisposableFixtureData data)
        {
            log.WhereAmI();
            disposableFixtureData = data;
            log.AppendLine("   DisposableFixtureData " + disposableFixtureData.Instance);
            data.Instance.ShouldEqual(1);
        }

        [Fact]
        public void ShouldAdd()
        {
            executedAddTest = true;
            log.WhereAmI();
            calculator.Add(2, 3).ShouldEqual(5);
        }

        [Fact]
        public void ShouldSubtract()
        {
            executedSubtractTest = true;
            log.WhereAmI();
            calculator.Subtract(5, 3).ShouldEqual(2);
        }

        public void Dispose()
        {
            log.WhereAmI();
            (executedAddTest && executedSubtractTest).ShouldBeFalse();
            (executedAddTest || executedSubtractTest).ShouldBeTrue();

            log.ShouldHaveLines(
                ".ctor",
                "SetFixture",
                "   FixtureData 1",
                "SetFixture",
                "   DisposableFixtureData 1",
                executedAddTest
                    ? "ShouldAdd"
                    : "ShouldSubtract",
                "Dispose");
        }
    }

    public class DisposableFixtureData : IDisposable
    {
        static int InstanceCounter;
        public static int DisposalCount { get; }

        public int Instance { get; }

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

        public int Instance { get; }

        public FixtureData()
        {
            Instance = ++InstanceCounter;
        }
    }
}