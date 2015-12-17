using System;

namespace Fixie.Tests.Lifecycle
{
    public class FixtureLifecycleTests : LifecycleTests
    {
        class Inner : FixtureBehavior
        {
            public void Execute(Fixture fixture, Action next)
            {
                Console.WriteLine("Inner Before");
                next();
                Console.WriteLine("Inner After");
            }
        }

        class Outer : FixtureBehavior
        {
            public void Execute(Fixture fixture, Action next)
            {
                Console.WriteLine("Outer Before");
                next();
                Console.WriteLine("Outer After");
            }
        }

        class DoNothing : FixtureBehavior
        {
            public void Execute(Fixture fixture, Action next)
            {
                //Behavior chooses not to invoke next().
                //Since the cases are never invoked, they don't
                //have the chance to throw exceptions, resulting
                //in all 'passing'.
            }
        }

        class ThrowException : FixtureBehavior
        {
            public void Execute(Fixture fixture, Action next)
            {
                Console.WriteLine("Unsafe fixture execution behavior");
                throw new Exception("Unsafe fixture execution behavior threw!");
            }
        }

        class ThrowPreservedException : FixtureBehavior
        {
            public void Execute(Fixture fixture, Action next)
            {
                Console.WriteLine("Unsafe fixture execution behavior");
                try
                {
                    throw new Exception("Unsafe fixture execution behavior threw!");
                }
                catch (Exception originalException)
                {
                    throw new PreservedException(originalException);
                }
            }
        }

        public void ShouldAllowWrappingFixtureWithBehaviorTypesWhenConstructingPerCase()
        {
            Convention.ClassExecution
                      .CreateInstancePerCase();

            Convention.FixtureExecution
                      .Wrap<Inner>()
                      .Wrap<Outer>();

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass passed",
                "SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                ".ctor",
                "Outer Before", "Inner Before",
                "Pass",
                "Inner After", "Outer After",
                "Dispose",
                ".ctor",
                "Outer Before", "Inner Before",
                "Fail",
                "Inner After", "Outer After",
                "Dispose");
        }

        public void ShouldAllowWrappingFixtureWithBehaviorTypesWhenConstructingPerClass()
        {
            Convention.ClassExecution
                      .CreateInstancePerClass();

            Convention.FixtureExecution
                      .Wrap<Inner>()
                      .Wrap<Outer>();

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass passed",
                "SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                ".ctor",
                "Outer Before", "Inner Before",
                "Pass", "Fail",
                "Inner After", "Outer After",
                "Dispose");
        }

        public void ShouldAllowWrappingFixtureWithBehaviorInstancesWhenConstructingPerCase()
        {
            Convention.ClassExecution
                      .CreateInstancePerCase();

            Convention.FixtureExecution
                      .Wrap(new Inner())
                      .Wrap(new Outer());

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass passed",
                "SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                ".ctor",
                "Outer Before", "Inner Before",
                "Pass",
                "Inner After", "Outer After",
                "Dispose",
                ".ctor",
                "Outer Before", "Inner Before",
                "Fail",
                "Inner After", "Outer After",
                "Dispose");
        }

        public void ShouldAllowWrappingFixtureWithBehaviorInstancesWhenConstructingPerClass()
        {
            Convention.ClassExecution
                      .CreateInstancePerClass();

            Convention.FixtureExecution
                      .Wrap(new Inner())
                      .Wrap(new Outer());

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass passed",
                "SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                ".ctor",
                "Outer Before", "Inner Before",
                "Pass", "Fail",
                "Inner After", "Outer After",
                "Dispose");
        }

        public void ShouldAllowWrappingFixtureWithBehaviorLambdasWhenConstructingPerCase()
        {
            Convention.ClassExecution
                      .CreateInstancePerCase();

            Convention.FixtureExecution
                      .Wrap((fixture, next) =>
                      {
                          Console.WriteLine("Inner Before");
                          next();
                          Console.WriteLine("Inner After");
                      })
                      .Wrap((fixture, next) =>
                      {
                          Console.WriteLine("Outer Before");
                          next();
                          Console.WriteLine("Outer After");
                      });

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass passed",
                "SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                ".ctor",
                "Outer Before", "Inner Before",
                "Pass",
                "Inner After", "Outer After",
                "Dispose",
                ".ctor",
                "Outer Before", "Inner Before",
                "Fail",
                "Inner After", "Outer After",
                "Dispose");
        }

        public void ShouldAllowWrappingFixtureWithBehaviorLambdasWhenConstructingPerClass()
        {
            Convention.ClassExecution
                      .CreateInstancePerClass();

            Convention.FixtureExecution
                      .Wrap((fixture, next) =>
                      {
                          Console.WriteLine("Inner Before");
                          next();
                          Console.WriteLine("Inner After");
                      })
                      .Wrap((fixture, next) =>
                      {
                          Console.WriteLine("Outer Before");
                          next();
                          Console.WriteLine("Outer After");
                      });

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass passed",
                "SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                ".ctor",
                "Outer Before", "Inner Before",
                "Pass", "Fail",
                "Inner After", "Outer After",
                "Dispose");
        }

        public void ShouldAllowFixtureBehaviorsToShortCircuitInnerBehaviorWhenConstructingPerCase()
        {
            Convention.ClassExecution
                      .CreateInstancePerCase();

            Convention.FixtureExecution
                      .Wrap<DoNothing>();

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass passed",
                "SampleTestClass.Fail passed");

            output.ShouldHaveLifecycle(
                ".ctor",
                "Dispose",
                ".ctor",
                "Dispose");
        }

        public void ShouldAllowFixtureBehaviorsToShortCircuitInnerBehaviorWhenConstructingPerClass()
        {
            Convention.ClassExecution
                      .CreateInstancePerClass();

            Convention.FixtureExecution
                      .Wrap<DoNothing>();

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass passed",
                "SampleTestClass.Fail passed");

            output.ShouldHaveLifecycle(
                ".ctor",
                "Dispose");
        }

        public void ShouldFailCaseWhenConstructingPerCaseAndFixtureBehaviorThrows()
        {
            Convention.ClassExecution
                      .CreateInstancePerCase();

            Convention.FixtureExecution
                      .Wrap<ThrowException>();

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: Unsafe fixture execution behavior threw!",
                "SampleTestClass.Fail failed: Unsafe fixture execution behavior threw!");

            output.ShouldHaveLifecycle(
                ".ctor",
                "Unsafe fixture execution behavior",
                "Dispose",
                ".ctor",
                "Unsafe fixture execution behavior",
                "Dispose");
        }

        public void ShouldFailAllCasesWhenConstructingPerClassAndFixtureBehaviorThrows()
        {
            Convention.ClassExecution
                      .CreateInstancePerClass();

            Convention.FixtureExecution
                      .Wrap<ThrowException>();

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: Unsafe fixture execution behavior threw!",
                "SampleTestClass.Fail failed: Unsafe fixture execution behavior threw!");

            output.ShouldHaveLifecycle(
                ".ctor",
                "Unsafe fixture execution behavior",
                "Dispose");
        }

        public void ShouldFailCaseWithOriginalExceptionWhenConstructingPerCaseAndFixtureBehaviorThrowsPreservedException()
        {
            Convention.ClassExecution
                      .CreateInstancePerCase();

            Convention.FixtureExecution
                      .Wrap<ThrowPreservedException>();

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: Unsafe fixture execution behavior threw!",
                "SampleTestClass.Fail failed: Unsafe fixture execution behavior threw!");

            output.ShouldHaveLifecycle(
                ".ctor",
                "Unsafe fixture execution behavior",
                "Dispose",
                ".ctor",
                "Unsafe fixture execution behavior",
                "Dispose");
        }

        public void ShouldFailAllCasesWithOriginalExceptionWhenConstructingPerClassAndFixtureBehaviorThrowsPreservedException()
        {
            Convention.ClassExecution
                      .CreateInstancePerClass();

            Convention.FixtureExecution
                      .Wrap<ThrowPreservedException>();

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: Unsafe fixture execution behavior threw!",
                "SampleTestClass.Fail failed: Unsafe fixture execution behavior threw!");

            output.ShouldHaveLifecycle(
                ".ctor",
                "Unsafe fixture execution behavior",
                "Dispose");
        }

        public void ShouldAllowWrappingFixtureWithSetUpTearDownBehaviorsWhenConstructingPerCase()
        {
            Convention.ClassExecution
                      .CreateInstancePerCase();

            Convention.FixtureExecution
                      .Wrap<FixtureSetUpTearDown>();

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass passed",
                "SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                ".ctor",
                "FixtureSetUp",
                "Pass",
                "FixtureTearDown",
                "Dispose",
                ".ctor",
                "FixtureSetUp",
                "Fail",
                "FixtureTearDown",
                "Dispose");
        }

        public void ShouldAllowWrappingFixtureWithSetUpTearDownBehaviorsWhenConstructingPerClass()
        {
            Convention.ClassExecution
                      .CreateInstancePerClass();

            Convention.FixtureExecution
                      .Wrap<FixtureSetUpTearDown>();

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass passed",
                "SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                ".ctor",
                "FixtureSetUp",
                "Pass", "Fail",
                "FixtureTearDown",
                "Dispose");
        }

        public void ShouldShortCircuitInnerBehaviorAndTearDownByFailingCaseWhenConstructingPerCaseAndFixtureSetUpThrows()
        {
            FailDuring("FixtureSetUp");

            Convention.ClassExecution
                      .CreateInstancePerCase();

            Convention.FixtureExecution
                      .Wrap<FixtureSetUpTearDown>();

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: 'FixtureSetUp' failed!",
                "SampleTestClass.Fail failed: 'FixtureSetUp' failed!");

            output.ShouldHaveLifecycle(
                ".ctor",
                "FixtureSetUp",
                "Dispose",
                ".ctor",
                "FixtureSetUp",
                "Dispose");
        }

        public void ShouldShortCircuitInnerBehaviorAndTearDownByFailingAllCasesWhenConstructingPerClassAndFixtureSetUpThrows()
        {
            FailDuring("FixtureSetUp");

            Convention.ClassExecution
                      .CreateInstancePerClass();

            Convention.FixtureExecution
                      .Wrap<FixtureSetUpTearDown>();

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: 'FixtureSetUp' failed!",
                "SampleTestClass.Fail failed: 'FixtureSetUp' failed!");

            output.ShouldHaveLifecycle(
                ".ctor",
                "FixtureSetUp",
                "Dispose");
        }

        public void ShouldFailCaseWhenConstructingPerCaseAndFixtureTearDownThrows()
        {
            FailDuring("FixtureTearDown");

            Convention.ClassExecution
                      .CreateInstancePerCase();

            Convention.FixtureExecution
                      .Wrap<FixtureSetUpTearDown>();

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: 'FixtureTearDown' failed!",
                "SampleTestClass.Fail failed: 'Fail' failed!" + Environment.NewLine +
                "    Secondary Failure: 'FixtureTearDown' failed!");

            output.ShouldHaveLifecycle(
                ".ctor",
                "FixtureSetUp",
                "Pass",
                "FixtureTearDown",
                "Dispose",
                ".ctor",
                "FixtureSetUp",
                "Fail",
                "FixtureTearDown",
                "Dispose");
        }

        public void ShouldFailAllCasesWhenConstructingPerClassAndFixtureTearDownThrows()
        {
            FailDuring("FixtureTearDown");

            Convention.ClassExecution
                      .CreateInstancePerClass();

            Convention.FixtureExecution
                      .Wrap<FixtureSetUpTearDown>();

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: 'FixtureTearDown' failed!",
                "SampleTestClass.Fail failed: 'Fail' failed!" + Environment.NewLine +
                "    Secondary Failure: 'FixtureTearDown' failed!");

            output.ShouldHaveLifecycle(
                ".ctor",
                "FixtureSetUp",
                "Pass", "Fail",
                "FixtureTearDown",
                "Dispose");
        }
    }
}