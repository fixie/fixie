using System;
using Should;

namespace Fixie.Tests.Lifecycle
{
    public class InstanceLifecycleTests : LifecycleTests
    {
        public void ShouldAllowWrappingInstanceExecutionWithCustomBehaviors()
        {
            Convention.ClassExecution
                      .CreateInstancePerTestClass();

            Convention.InstanceExecution
                      .Wrap((fixture, innerBehavior) =>
                      {
                          Console.WriteLine("Inner Before");
                          innerBehavior();
                          Console.WriteLine("Inner After");
                      })
                      .Wrap((fixture, innerBehavior) =>
                      {
                          Console.WriteLine("Outer Before");
                          innerBehavior();
                          Console.WriteLine("Outer After");
                      });

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

        public void ShouldAllowCustomBehaviorsToShortCircuitInnerBehavior()
        {
            Convention.ClassExecution
                      .CreateInstancePerCase();

            Convention.InstanceExecution
                      .Wrap((fixture, innerBehavior) =>
                      {
                          //Behavior chooses not to invoke innerBehavior().
                          //Since the cases are never invoked, they don't
                          //have the chance to throw exceptions, resulting
                          //in all 'passing'.
                      });

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

        public void ShouldFailAllCasesWhenInstanceExecutionCustomBehaviorThrows()
        {
            Convention.ClassExecution
                      .CreateInstancePerTestClass();

            Convention.InstanceExecution
                      .Wrap((fixture, innerBehavior) =>
                      {
                          Console.WriteLine("Unsafe instance execution behavior");
                          throw new Exception("Unsafe instance execution behavior threw!");
                      });

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: Unsafe instance execution behavior threw!",
                "SampleTestClass.Fail failed: Unsafe instance execution behavior threw!");

            output.ShouldHaveLifecycle(
                ".ctor",
                "Unsafe instance execution behavior",
                "Dispose");
        }

        public void ShouldAllowWrappingInstanceExecutionWithSetUpTearDownBehaviors()
        {
            Convention.ClassExecution
                      .CreateInstancePerTestClass();

            Convention.InstanceExecution
                      .SetUpTearDown(SetUp, TearDown);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass passed.",
                "SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                ".ctor",
                "SetUp",
                "Pass", "Fail",
                "TearDown",
                "Dispose");
        }

        public void ShouldShortCircuitInnerBehaviorAndTearDownByFailingAllCasesWhenInstanceExecutionSetUpThrows()
        {
            FailDuring("SetUp");

            Convention.ClassExecution
                      .CreateInstancePerTestClass();

            Convention.InstanceExecution
                      .SetUpTearDown(SetUp, TearDown);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: 'SetUp' failed!",
                "SampleTestClass.Fail failed: 'SetUp' failed!");

            output.ShouldHaveLifecycle(
                ".ctor",
                "SetUp",
                "Dispose");
        }

        public void ShouldFailAllCasesWhenInstanceExecutionTearDownThrows()
        {
            FailDuring("TearDown");

            Convention.ClassExecution
                      .CreateInstancePerTestClass();

            Convention.InstanceExecution
                      .SetUpTearDown(SetUp, TearDown);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: 'TearDown' failed!",
                "SampleTestClass.Fail failed: 'Fail' failed!" + Environment.NewLine +
                "    Secondary Failure: 'TearDown' failed!");

            output.ShouldHaveLifecycle(
                ".ctor",
                "SetUp",
                "Pass", "Fail",
                "TearDown",
                "Dispose");
        }
        
        static void SetUp(Fixture fixture)
        {
            fixture.Cases.Length.ShouldEqual(2);
            fixture.TestClass.ShouldEqual(typeof(SampleTestClass));
            WhereAmI();
        }

        static void TearDown(Fixture fixture)
        {
            fixture.Cases.Length.ShouldEqual(2);
            fixture.TestClass.ShouldEqual(typeof(SampleTestClass));
            WhereAmI();
        }
    }
}