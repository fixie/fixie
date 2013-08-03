using System;
using Should;

namespace Fixie.Tests.Lifecycle
{
    public class ClassExecutionTests : LifecycleTests
    {
        public void ShouldAllowWrappingClassWithCustomBehaviorsWhenConstructingPerCase()
        {
            Convention.ClassExecution
                      .CreateInstancePerCase()
                      .Wrap((testClass, conv, cases, innerBehavior) =>
                      {
                          Console.WriteLine("Inner Before");
                          innerBehavior();
                          Console.WriteLine("Inner After");
                      })
                      .Wrap((testClass, conv, cases, innerBehavior) =>
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
                "Outer Before", "Inner Before",
                ".ctor", "Pass", "Dispose",
                ".ctor", "Fail", "Dispose",
                "Inner After", "Outer After");
        }

        public void ShouldAllowWrappingClassWithCustomBehaviorsWhenConstructingPerTestClass()
        {
            Convention.ClassExecution
                      .CreateInstancePerTestClass()
                      .Wrap((testClass, conv, cases, innerBehavior) =>
                      {
                          Console.WriteLine("Inner Before");
                          innerBehavior();
                          Console.WriteLine("Inner After");
                      })
                      .Wrap((testClass, conv, cases, innerBehavior) =>
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
                "Outer Before", "Inner Before",
                ".ctor", "Pass", "Fail", "Dispose",
                "Inner After", "Outer After");
        }

        public void ShouldAllowCustomBehaviorsToShortCircuitInnerBehaviorWhenConstructingPerCase()
        {
            Convention.ClassExecution
                      .CreateInstancePerCase()
                      .Wrap((testClass, conv, cases, innerBehavior) =>
                      {
                          //Behavior chooses not to invoke innerBehavior().
                          //Since the test classes are never intantiated,
                          //their cases don't have the chance to throw exceptions,
                          //resulting in all 'passing'.
                      });

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass passed.",
                "SampleTestClass.Fail passed.");

            output.ShouldHaveLifecycle();
        }

        public void ShouldAllowCustomBehaviorsToShortCircuitInnerBehaviorWhenConstructingPerTestClass()
        {
            Convention.ClassExecution
                      .CreateInstancePerTestClass()
                      .Wrap((testClass, conv, cases, innerBehavior) =>
                      {
                          //Behavior chooses not to invoke innerBehavior().
                          //Since the test classes are never intantiated,
                          //their cases don't have the chance to throw exceptions,
                          //resulting in all 'passing'.
                      });

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass passed.",
                "SampleTestClass.Fail passed.");

            output.ShouldHaveLifecycle();
        }

        public void ShouldFailCaseWhenConstructingPerCaseAndCustomBehaviorThrows()
        {
            Convention.ClassExecution
                      .CreateInstancePerCase()
                      .Wrap((testClass, conv, cases, innerBehavior) =>
                      {
                          Console.WriteLine("Unsafe class execution behavior");
                          throw new Exception("Unsafe class execution behavior threw!");
                      });

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: Unsafe class execution behavior threw!",
                "SampleTestClass.Fail failed: Unsafe class execution behavior threw!");

            output.ShouldHaveLifecycle("Unsafe class execution behavior");
        }

        public void ShouldFailAllCasesWhenConstructingPerTestClassAndCustomBehaviorThrows()
        {
            Convention.ClassExecution
                      .CreateInstancePerTestClass()
                      .Wrap((testClass, conv, cases, innerBehavior) =>
                      {
                          Console.WriteLine("Unsafe class execution behavior");
                          throw new Exception("Unsafe class execution behavior threw!");
                      });

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: Unsafe class execution behavior threw!",
                "SampleTestClass.Fail failed: Unsafe class execution behavior threw!");

            output.ShouldHaveLifecycle("Unsafe class execution behavior");
        }

        public void ShouldAllowWrappingClassWithSetUpTearDownBehaviorsWhenConstructingPerCase()
        {
            Convention.ClassExecution
                      .CreateInstancePerCase()
                      .SetUpTearDown(SetUp, TearDown);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass passed.",
                "SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                "SetUp",
                ".ctor", "Pass", "Dispose",
                ".ctor", "Fail", "Dispose",
                "TearDown");
        }

        public void ShouldAllowWrappingClassWithSetUpTearDownBehaviorsWhenConstructingPerTestClass()
        {
            Convention.ClassExecution
                      .CreateInstancePerTestClass()
                      .SetUpTearDown(SetUp, TearDown);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass passed.",
                "SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                "SetUp",
                ".ctor", "Pass", "Fail", "Dispose",
                "TearDown");
        }

        public void ShouldShortCircuitInnerBehaviorAndTearDownByFailingCaseWhenConstructingPerCaseAndSetUpThrows()
        {
            FailDuring("SetUp");

            Convention.ClassExecution
                      .CreateInstancePerCase()
                      .SetUpTearDown(SetUp, TearDown);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: 'SetUp' failed!",
                "SampleTestClass.Fail failed: 'SetUp' failed!");

            output.ShouldHaveLifecycle(
                "SetUp");
        }

        public void ShouldShortCircuitInnerBehaviorAndTearDownByFailingAllCasesWhenConstructingPerTestClassAndSetUpThrows()
        {
            FailDuring("SetUp");

            Convention.ClassExecution
                      .CreateInstancePerTestClass()
                      .SetUpTearDown(SetUp, TearDown);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: 'SetUp' failed!",
                "SampleTestClass.Fail failed: 'SetUp' failed!");

            output.ShouldHaveLifecycle(
                "SetUp");
        }

        public void ShouldFailCaseWhenConstructingPerCaseAndTearDownThrows()
        {
            FailDuring("TearDown");

            Convention.ClassExecution
                      .CreateInstancePerCase()
                      .SetUpTearDown(SetUp, TearDown);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: 'TearDown' failed!",
                "SampleTestClass.Fail failed: 'Fail' failed!" + Environment.NewLine +
                "    Secondary Failure: 'TearDown' failed!");

            output.ShouldHaveLifecycle(
                "SetUp",
                ".ctor", "Pass", "Dispose",
                ".ctor", "Fail", "Dispose",
                "TearDown");
        }

        public void ShouldFailAllCasesWhenConstructingPerTestClassAndTearDownThrows()
        {
            FailDuring("TearDown");

            Convention.ClassExecution
                      .CreateInstancePerTestClass()
                      .SetUpTearDown(SetUp, TearDown);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: 'TearDown' failed!",
                "SampleTestClass.Fail failed: 'Fail' failed!" + Environment.NewLine +
                "    Secondary Failure: 'TearDown' failed!");

            output.ShouldHaveLifecycle(
                "SetUp",
                ".ctor", "Pass", "Fail", "Dispose",
                "TearDown");
        }

        static void SetUp(Type testClass)
        {
            testClass.ShouldEqual(typeof(SampleTestClass));
            WhereAmI();
        }

        static void TearDown(Type testClass)
        {
            testClass.ShouldEqual(typeof(SampleTestClass));
            WhereAmI();
        }
    }
}