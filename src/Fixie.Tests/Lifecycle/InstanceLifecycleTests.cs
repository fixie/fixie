using System;

namespace Fixie.Tests.Lifecycle
{
    public class InstanceLifecycleTests : LifecycleTests
    {
        public void ShouldAllowWrappingInstanceWithBehaviorsWhenConstructingPerCase()
        {
            Convention.ClassExecution
                      .CreateInstancePerCase();

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
                "Pass",
                "Inner After", "Outer After",
                "Dispose",
                ".ctor",
                "Outer Before", "Inner Before",
                "Fail",
                "Inner After", "Outer After",
                "Dispose");
        }

        public void ShouldAllowWrappingInstanceWithBehaviorsWhenConstructingPerTestClass()
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

        public void ShouldAllowInstanceBehaviorsToShortCircuitInnerBehaviorWhenConstructingPerCase()
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

        public void ShouldAllowInstanceBehaviorsToShortCircuitInnerBehaviorWhenConstructingPerTestClass()
        {
            Convention.ClassExecution
                      .CreateInstancePerTestClass();

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
                "Dispose");
        }

        public void ShouldFailCaseWhenConstructingPerCaseAndInstanceBehaviorThrows()
        {
            Convention.ClassExecution
                      .CreateInstancePerCase();

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
                "Dispose",
                ".ctor",
                "Unsafe instance execution behavior",
                "Dispose");
        }

        public void ShouldFailAllCasesWhenConstructingPerTestClassAndInstanceBehaviorThrows()
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

        public void ShouldFailCaseWithOriginalExceptionWhenConstructingPerCaseAndInstanceBehaviorThrowsPreservedException()
        {
            Convention.ClassExecution
                      .CreateInstancePerCase();

            Convention.InstanceExecution
                      .Wrap((fixture, innerBehavior) =>
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
                      });

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

        public void ShouldFailAllCasesWithOriginalExceptionWhenConstructingPerTestClassAndInstanceBehaviorThrowsPreservedException()
        {
            Convention.ClassExecution
                      .CreateInstancePerTestClass();

            Convention.InstanceExecution
                      .Wrap((fixture, innerBehavior) =>
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

        public void ShouldAllowWrappingInstanceWithSetUpTearDownBehaviorsWhenConstructingPerCase()
        {
            Convention.ClassExecution
                      .CreateInstancePerCase();

            Convention.InstanceExecution
                      .SetUpTearDown(InstanceSetUp, InstanceTearDown);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass passed.",
                "SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                ".ctor",
                "InstanceSetUp",
                "Pass",
                "InstanceTearDown",
                "Dispose",
                ".ctor",
                "InstanceSetUp",
                "Fail",
                "InstanceTearDown",
                "Dispose");
        }

        public void ShouldAllowWrappingInstanceWithSetUpTearDownBehaviorsWhenConstructingPerTestClass()
        {
            Convention.ClassExecution
                      .CreateInstancePerTestClass();

            Convention.InstanceExecution
                      .SetUpTearDown(InstanceSetUp, InstanceTearDown);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass passed.",
                "SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                ".ctor",
                "InstanceSetUp",
                "Pass", "Fail",
                "InstanceTearDown",
                "Dispose");
        }

        public void ShouldShortCircuitInnerBehaviorAndTearDownByFailingCaseWhenConstructingPerCaseAndInstanceSetUpThrows()
        {
            FailDuring("InstanceSetUp");

            Convention.ClassExecution
                      .CreateInstancePerCase();

            Convention.InstanceExecution
                      .SetUpTearDown(InstanceSetUp, InstanceTearDown);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: 'InstanceSetUp' failed!",
                "SampleTestClass.Fail failed: 'InstanceSetUp' failed!");

            output.ShouldHaveLifecycle(
                ".ctor",
                "InstanceSetUp",
                "Dispose",
                ".ctor",
                "InstanceSetUp",
                "Dispose");
        }

        public void ShouldShortCircuitInnerBehaviorAndTearDownByFailingAllCasesWhenConstructingPerTestClassAndInstanceSetUpThrows()
        {
            FailDuring("InstanceSetUp");

            Convention.ClassExecution
                      .CreateInstancePerTestClass();

            Convention.InstanceExecution
                      .SetUpTearDown(InstanceSetUp, InstanceTearDown);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: 'InstanceSetUp' failed!",
                "SampleTestClass.Fail failed: 'InstanceSetUp' failed!");

            output.ShouldHaveLifecycle(
                ".ctor",
                "InstanceSetUp",
                "Dispose");
        }

        public void ShouldFailCaseWhenConstructingPerCaseAndInstanceTearDownThrows()
        {
            FailDuring("InstanceTearDown");

            Convention.ClassExecution
                      .CreateInstancePerCase();

            Convention.InstanceExecution
                      .SetUpTearDown(InstanceSetUp, InstanceTearDown);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: 'InstanceTearDown' failed!",
                "SampleTestClass.Fail failed: 'Fail' failed!" + Environment.NewLine +
                "    Secondary Failure: 'InstanceTearDown' failed!");

            output.ShouldHaveLifecycle(
                ".ctor",
                "InstanceSetUp",
                "Pass",
                "InstanceTearDown",
                "Dispose",
                ".ctor",
                "InstanceSetUp",
                "Fail",
                "InstanceTearDown",
                "Dispose");
        }

        public void ShouldFailAllCasesWhenConstructingPerTestClassAndInstanceTearDownThrows()
        {
            FailDuring("InstanceTearDown");

            Convention.ClassExecution
                      .CreateInstancePerTestClass();

            Convention.InstanceExecution
                      .SetUpTearDown(InstanceSetUp, InstanceTearDown);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: 'InstanceTearDown' failed!",
                "SampleTestClass.Fail failed: 'Fail' failed!" + Environment.NewLine +
                "    Secondary Failure: 'InstanceTearDown' failed!");

            output.ShouldHaveLifecycle(
                ".ctor",
                "InstanceSetUp",
                "Pass", "Fail",
                "InstanceTearDown",
                "Dispose");
        }

        public void ShouldAllowWrappingInstanceWithSetUpBehaviorWhenConstructingPerCase()
        {
            Convention.ClassExecution
                      .CreateInstancePerCase();

            Convention.InstanceExecution
                      .SetUp(InstanceSetUp);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass passed.",
                "SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                ".ctor",
                "InstanceSetUp",
                "Pass",
                "Dispose",
                ".ctor",
                "InstanceSetUp",
                "Fail",
                "Dispose");
        }

        public void ShouldAllowWrappingInstanceWithSetUpBehaviorWhenConstructingPerTestClass()
        {
            Convention.ClassExecution
                      .CreateInstancePerTestClass();

            Convention.InstanceExecution
                      .SetUp(InstanceSetUp);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass passed.",
                "SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                ".ctor",
                "InstanceSetUp",
                "Pass",
                "Fail",
                "Dispose");
        }

        public void ShouldShortCircuitInnerBehaviorByFailingCaseWhenConstructingPerCaseAndInstanceSetUpThrows()
        {
            FailDuring("InstanceSetUp");

            Convention.ClassExecution
                      .CreateInstancePerCase();

            Convention.InstanceExecution
                      .SetUp(InstanceSetUp);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: 'InstanceSetUp' failed!",
                "SampleTestClass.Fail failed: 'InstanceSetUp' failed!");

            output.ShouldHaveLifecycle(
                ".ctor",
                "InstanceSetUp",
                "Dispose",
                ".ctor",
                "InstanceSetUp",
                "Dispose");
        }

        public void ShouldShortCircuitInnerBehaviorByFailingAllCasesWhenConstructingPerTestClassAndInstanceSetUpThrows()
        {
            FailDuring("InstanceSetUp");

            Convention.ClassExecution
                      .CreateInstancePerTestClass();

            Convention.InstanceExecution
                      .SetUp(InstanceSetUp);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: 'InstanceSetUp' failed!",
                "SampleTestClass.Fail failed: 'InstanceSetUp' failed!");

            output.ShouldHaveLifecycle(
                ".ctor",
                "InstanceSetUp",
                "Dispose");
        }
    }
}