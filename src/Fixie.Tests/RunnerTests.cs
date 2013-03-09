using System;
using NUnit.Framework;
using Shouldly;

namespace Fixie.Tests
{
    [TestFixture]
    public class RunnerTests
    {
        [Test]
        public void ShouldExecuteAllCasesInTheGivenSuite()
        {
            var suite = new Suite(typeof(Fixture1Tests), typeof(Fixture2Tests));
            var listener = new StubListener();
            var runner = new Runner(listener);

            var result = runner.Execute(suite);

            result.Passed.ShouldBe(3);
            result.Failed.ShouldBe(1);
        }

        [Test]
        public void ShouldLogFailingCases()
        {
            var suite = new Suite(typeof(Fixture1Tests), typeof(Fixture2Tests));
            var listener = new StubListener();
            var runner = new Runner(listener);

            runner.Execute(suite);

            listener.ToString().ShouldBe("SampleTestFail failed: Exception!");
        }

        private class Fixture1Tests
        {
            public void SampleTestFail() { throw new Exception("Exception!"); }
            public void SampleTestPass() { }
        }

        private class Fixture2Tests
        {
            public void SampleTestPassA() { }
            public void SampleTestPassB() { }
        }
    }
}