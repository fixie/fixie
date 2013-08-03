using System;
using Should;

namespace Fixie.Tests.Lifecycle
{
    public class CaseLifecycleTests : LifecycleTests
    {
        public void ShouldAllowWrappingCaseExecutionWithCustomBehaviors()
        {
            Convention.ClassExecution
                      .CreateInstancePerTestClass();

            Convention.CaseExecution
                      .Wrap((@case, instance, innerBehavior) =>
                      {
                          Console.WriteLine("Inner Before");
                          innerBehavior();
                          Console.WriteLine("Inner After");
                      })
                      .Wrap((@case, instance, innerBehavior) =>
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
                "Pass",
                "Inner After", "Outer After",
                "Outer Before", "Inner Before",
                "Fail",
                "Inner After", "Outer After",
                "Dispose");
        }

        public void ShouldAllowCustomBehaviorsToShortCircuitInnerBehavior()
        {
            Convention.ClassExecution
                      .CreateInstancePerTestClass();

            Convention.CaseExecution
                      .Wrap((@case, instance, innerBehavior) =>
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
                "Dispose");
        }

        public void ShouldFailAllCasesWhenCaseExecutionCustomBehaviorThrows()
        {
            Convention.ClassExecution
                      .CreateInstancePerTestClass();

            Convention.CaseExecution
                      .Wrap((@case, instance, innerBehavior) =>
                      {
                          Console.WriteLine("Unsafe case execution behavior");
                          throw new Exception("Unsafe case execution behavior threw!");
                      });

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: Unsafe case execution behavior threw!",
                "SampleTestClass.Fail failed: Unsafe case execution behavior threw!");

            output.ShouldHaveLifecycle(
                ".ctor",
                "Unsafe case execution behavior",
                "Unsafe case execution behavior",
                "Dispose");
        }

        public void ShouldAllowWrappingCaseExecutionWithSetUpTearDownBehaviors()
        {
            Convention.ClassExecution
                      .CreateInstancePerTestClass();

            Convention.CaseExecution
                      .SetUpTearDown(SetUp, TearDown);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass passed.",
                "SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                ".ctor",
                "SetUp", "Pass", "TearDown",
                "SetUp", "Fail", "TearDown",
                "Dispose");
        }

        public void ShouldShortCircuitInnerBehaviorAndTearDownByFailingAllCasesWhenCaseExecutionSetUpThrows()
        {
            FailDuring("SetUp");

            Convention.ClassExecution
                      .CreateInstancePerTestClass();

            Convention.CaseExecution
                      .SetUpTearDown(SetUp, TearDown);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: 'SetUp' failed!",
                "SampleTestClass.Fail failed: 'SetUp' failed!");

            output.ShouldHaveLifecycle(
                ".ctor",
                "SetUp",
                "SetUp",
                "Dispose");
        }

        public void ShouldFailAllCasesWhenCaseExecutionTearDownThrows()
        {
            FailDuring("TearDown");

            Convention.ClassExecution
                      .CreateInstancePerTestClass();

            Convention.CaseExecution
                      .SetUpTearDown(SetUp, TearDown);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: 'TearDown' failed!",
                "SampleTestClass.Fail failed: 'Fail' failed!" + Environment.NewLine +
                "    Secondary Failure: 'TearDown' failed!");

            output.ShouldHaveLifecycle(
                ".ctor",
                "SetUp", "Pass", "TearDown",
                "SetUp", "Fail", "TearDown",
                "Dispose");
        }

        static void SetUp(Case @case, object instance)
        {
            @case.Class.ShouldEqual(typeof(SampleTestClass));
            instance.ShouldBeType<SampleTestClass>();
            WhereAmI();
        }

        static void TearDown(Case @case, object instance)
        {
            @case.Class.ShouldEqual(typeof(SampleTestClass));
            instance.ShouldBeType<SampleTestClass>();
            WhereAmI();
        }
    }
}