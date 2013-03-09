using System;
using System.Collections.Generic;
using NUnit.Framework;
using Shouldly;

namespace Fixie.Tests
{
    [TestFixture]
    public class RunnerTests
    {
        [Test]
        public void ShouldExecuteAllCasesInTheGivenConfiguration()
        {
            var configuration = new StubConfiguration();
            var listener = new StubListener();
            var runner = new Runner(listener);

            var result = runner.Execute(configuration);

            result.Total.ShouldBe(4);
            result.Passed.ShouldBe(3);
            result.Failed.ShouldBe(1);
        }

        [Test]
        public void ShouldLogFailingCases()
        {
            var configuration = new StubConfiguration();
            var listener = new StubListener();
            var runner = new Runner(listener);

            runner.Execute(configuration);

            listener.ToString().ShouldBe("Failing Case failed: Exception!");
        }

        class StubConfiguration : Configuration
        {
            public StubConfiguration()
            {
                Fixtures = new[]
                {
                    new StubFixture("Fixture 1",
                                    new StubCase("Failing Case", () => { throw new Exception("Exception!"); }),
                                    new StubCase("Passing Case")),
                    new StubFixture("Fixture 2",
                                    new StubCase("Passing Case A"),
                                    new StubCase("Passing Case B"))
                };
            }

            public IEnumerable<Fixture> Fixtures { get; private set; }
        }

        class StubFixture : Fixture
        {
            public StubFixture(string name, params StubCase[] cases)
            {
                Name = name;
                Cases = cases;
            }

            public string Name { get; private set; }
            public IEnumerable<Case> Cases { get; private set; }
        }

        class StubCase : Case
        {
            readonly Action execute;

            public StubCase(string name)
                :this(name, () => { }) { }

            public StubCase(string name, Action executionAction)
            {
                Name = name;
                execute = executionAction;
            }

            public string Name { get; private set; }

            public void Execute()
            {
                execute();
            }
        }
    }
}