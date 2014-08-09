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
                Console.WriteLine("Unsafe instance execution behavior");
                throw new Exception("Unsafe instance execution behavior threw!");
            }
        }

        class ThrowPreservedException : FixtureBehavior
        {
            public void Execute(Fixture fixture, Action next)
            {
                Console.WriteLine("Unsafe instance execution behavior");
                try
                {
                    throw new Exception("Unsafe instance execution behavior threw!");
                }
                catch (Exception originalException)
                {
                    throw new PreservedException(originalException);
                }
            }
        }

        public void ShouldAllowWrappingInstanceWithBehaviorsWhenConstructingPerCase()
        {
            Convention.ClassExecution
                      .CreateInstancePerCase();

            Convention.InstanceExecution
                      .Wrap<Inner>()
                      .Wrap<Outer>();

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass passed.",
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

        public void ShouldAllowWrappingInstanceWithBehaviorsWhenConstructingPerClass()
        {
            Convention.ClassExecution
                      .CreateInstancePerClass();

            Convention.InstanceExecution
                      .Wrap<Inner>()
                      .Wrap<Outer>();

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass passed.",
                "SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                ".ctor",
                "Outer Before", "Inner Before",
                "Pass", "Fail",
                "Inner After", "Outer After",
                "Dispose");
        }

        public void ShouldAllowInstanceBehaviorsToShortCircuitInnerBehaviorWhenConstructingPerCase()
        {
            Convention.ClassExecution
                      .CreateInstancePerCase();

            Convention.InstanceExecution
                      .Wrap<DoNothing>();

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass passed.",
                "SampleTestClass.Fail passed.");

            output.ShouldHaveLifecycle(
                ".ctor",
                "Dispose",
                ".ctor",
                "Dispose");
        }

        public void ShouldAllowInstanceBehaviorsToShortCircuitInnerBehaviorWhenConstructingPerClass()
        {
            Convention.ClassExecution
                      .CreateInstancePerClass();

            Convention.InstanceExecution
                      .Wrap<DoNothing>();

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass passed.",
                "SampleTestClass.Fail passed.");

            output.ShouldHaveLifecycle(
                ".ctor",
                "Dispose");
        }

        public void ShouldFailCaseWhenConstructingPerCaseAndInstanceBehaviorThrows()
        {
            Convention.ClassExecution
                      .CreateInstancePerCase();

            Convention.InstanceExecution
                      .Wrap<ThrowException>();

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: Unsafe instance execution behavior threw!",
                "SampleTestClass.Fail failed: Unsafe instance execution behavior threw!");

            output.ShouldHaveLifecycle(
                ".ctor",
                "Unsafe instance execution behavior",
                "Dispose",
                ".ctor",
                "Unsafe instance execution behavior",
                "Dispose");
        }

        public void ShouldFailAllCasesWhenConstructingPerClassAndInstanceBehaviorThrows()
        {
            Convention.ClassExecution
                      .CreateInstancePerClass();

            Convention.InstanceExecution
                      .Wrap<ThrowException>();

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: Unsafe instance execution behavior threw!",
                "SampleTestClass.Fail failed: Unsafe instance execution behavior threw!");

            output.ShouldHaveLifecycle(
                ".ctor",
                "Unsafe instance execution behavior",
                "Dispose");
        }

        public void ShouldFailCaseWithOriginalExceptionWhenConstructingPerCaseAndInstanceBehaviorThrowsPreservedException()
        {
            Convention.ClassExecution
                      .CreateInstancePerCase();

            Convention.InstanceExecution
                      .Wrap<ThrowPreservedException>();

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: Unsafe instance execution behavior threw!",
                "SampleTestClass.Fail failed: Unsafe instance execution behavior threw!");

            output.ShouldHaveLifecycle(
                ".ctor",
                "Unsafe instance execution behavior",
                "Dispose",
                ".ctor",
                "Unsafe instance execution behavior",
                "Dispose");
        }

        public void ShouldFailAllCasesWithOriginalExceptionWhenConstructingPerClassAndInstanceBehaviorThrowsPreservedException()
        {
            Convention.ClassExecution
                      .CreateInstancePerClass();

            Convention.InstanceExecution
                      .Wrap<ThrowPreservedException>();

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: Unsafe instance execution behavior threw!",
                "SampleTestClass.Fail failed: Unsafe instance execution behavior threw!");

            output.ShouldHaveLifecycle(
                ".ctor",
                "Unsafe instance execution behavior",
                "Dispose");
        }

        public void ShouldAllowWrappingInstanceWithSetUpTearDownBehaviorsWhenConstructingPerCase()
        {
            Convention.ClassExecution
                      .CreateInstancePerCase();

            Convention.InstanceExecution
                      .Wrap<FixtureSetUpTearDown>();

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass passed.",
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

        public void ShouldAllowWrappingInstanceWithSetUpTearDownBehaviorsWhenConstructingPerClass()
        {
            Convention.ClassExecution
                      .CreateInstancePerClass();

            Convention.InstanceExecution
                      .Wrap<FixtureSetUpTearDown>();

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass passed.",
                "SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                ".ctor",
                "FixtureSetUp",
                "Pass", "Fail",
                "FixtureTearDown",
                "Dispose");
        }

        public void ShouldShortCircuitInnerBehaviorAndTearDownByFailingCaseWhenConstructingPerCaseAndInstanceSetUpThrows()
        {
            FailDuring("FixtureSetUp");

            Convention.ClassExecution
                      .CreateInstancePerCase();

            Convention.InstanceExecution
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

        public void ShouldShortCircuitInnerBehaviorAndTearDownByFailingAllCasesWhenConstructingPerClassAndInstanceSetUpThrows()
        {
            FailDuring("FixtureSetUp");

            Convention.ClassExecution
                      .CreateInstancePerClass();

            Convention.InstanceExecution
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

        public void ShouldFailCaseWhenConstructingPerCaseAndInstanceTearDownThrows()
        {
            FailDuring("FixtureTearDown");

            Convention.ClassExecution
                      .CreateInstancePerCase();

            Convention.InstanceExecution
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

        public void ShouldFailAllCasesWhenConstructingPerClassAndInstanceTearDownThrows()
        {
            FailDuring("FixtureTearDown");

            Convention.ClassExecution
                      .CreateInstancePerClass();

            Convention.InstanceExecution
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