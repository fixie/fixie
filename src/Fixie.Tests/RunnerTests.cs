using System;
using System.Collections.Generic;
using Should;
using Xunit;

namespace Fixie.Tests
{
    public class RunnerTests
    {
        [Fact]
        public void ShouldExecuteAllCasesFoundByTheGivenConvention()
        {
            var convention = new StubConvention();
            var listener = new StubListener();
            var runner = new Runner(listener);

            var result = runner.Execute(convention);

            result.Total.ShouldEqual(5);
            result.Passed.ShouldEqual(3);
            result.Failed.ShouldEqual(2);
        }

        [Fact]
        public void ShouldLogFailedCaseExecution()
        {
            var convention = new StubConvention();
            var listener = new StubListener();
            var runner = new Runner(listener);

            runner.Execute(convention);

            listener.Entries.ShouldEqual("Throwing Case failed: Uncaught Exception!");
        }

        class StubConvention : Convention
        {
            public StubConvention()
            {
                Fixtures = new[]
                {
                    new StubFixture("Fixture 1",
                                    new StubCase("Throwing Case", listener => { throw new Exception("Uncaught Exception!"); }),
                                    new StubCase("Failing Case", listener => Result.Fail),
                                    new StubCase("Passing Case", listener => Result.Pass)),
                    new StubFixture("Fixture 2",
                                    new StubCase("Passing Case A", listener => Result.Pass),
                                    new StubCase("Passing Case B", listener => Result.Pass))
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
            readonly Func<Listener, Result> execute;

            public StubCase(string name, Func<Listener, Result> execute)
            {
                Name = name;
                this.execute = execute;
            }

            public string Name { get; private set; }

            public Result Execute(Listener listener)
            {
                return execute(listener);
            }
        }
    }
}