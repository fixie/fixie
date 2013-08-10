using System;

namespace Fixie.Tests.Lifecycle
{
    public class CaseLifecycleTests : LifecycleTests
    {
        public void ShouldAllowWrappingCaseWithBehaviorsWhenConstructingPerCase()
        {
            Convention.ClassExecution
                      .CreateInstancePerCase();

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
                "Dispose",
                ".ctor",
                "Outer Before", "Inner Before",
                "Fail",
                "Inner After", "Outer After",
                "Dispose");
        }

        public void ShouldAllowWrappingCaseWithBehaviorsWhenConstructingPerTestClass()
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

        public void ShouldAllowCaseBehaviorsToShortCircuitInnerBehaviorWhenConstructingPerCase()
        {
            Convention.ClassExecution
                      .CreateInstancePerCase();

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
                "Dispose",
                ".ctor",
                "Dispose");
        }
        
        public void ShouldAllowCaseBehaviorsToShortCircuitInnerBehaviorWhenConstructingPerTestClass()
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

        public void ShouldFailCaseWhenConstructingPerCaseAndCaseBehaviorThrows()
        {
            Convention.ClassExecution
                      .CreateInstancePerCase();

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
                "Dispose",
                ".ctor",
                "Unsafe case execution behavior",
                "Dispose");
        }

        public void ShouldFailAllCasesWhenConstructingPerTestClassAndCaseBehaviorThrows()
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

        public void ShouldFailCaseWithOriginalExceptionWhenConstructingPerCaseAndCaseBehaviorThrowsPreservedException()
        {
            Convention.ClassExecution
                      .CreateInstancePerCase();

            Convention.CaseExecution
                      .Wrap((@case, instance, innerBehavior) =>
                      {
                          Console.WriteLine("Unsafe case execution behavior");
                          try
                          {
                              throw new Exception("Unsafe case execution behavior threw!");
                          }
                          catch (Exception originalException)
                          {
                              throw new PreservedException(originalException);
                          }
                      });

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: Unsafe case execution behavior threw!",
                "SampleTestClass.Fail failed: Unsafe case execution behavior threw!");

            output.ShouldHaveLifecycle(
                ".ctor",
                "Unsafe case execution behavior",
                "Dispose",
                ".ctor",
                "Unsafe case execution behavior",
                "Dispose");
        }

        public void ShouldFailAllCasesWithOriginalExceptionWhenConstructingPerTestClassAndCaseBehaviorThrowsPreservedException()
        {
            Convention.ClassExecution
                      .CreateInstancePerTestClass();

            Convention.CaseExecution
                      .Wrap((@case, instance, innerBehavior) =>
                      {
                          Console.WriteLine("Unsafe case execution behavior");
                          try
                          {
                              throw new Exception("Unsafe case execution behavior threw!");
                          }
                          catch (Exception originalException)
                          {
                              throw new PreservedException(originalException);
                          }
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

        public void ShouldAllowWrappingCaseWithSetUpTearDownBehaviorsWhenConstructingPerCase()
        {
            Convention.ClassExecution
                      .CreateInstancePerCase();

            Convention.CaseExecution
                      .SetUpTearDown(CaseSetUp, CaseTearDown);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass passed.",
                "SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                ".ctor",
                "CaseSetUp", "Pass", "CaseTearDown",
                "Dispose",
                ".ctor",
                "CaseSetUp", "Fail", "CaseTearDown",
                "Dispose");
        }

        public void ShouldAllowWrappingCaseWithSetUpTearDownBehaviorsWhenConstructingPerTestClass()
        {
            Convention.ClassExecution
                      .CreateInstancePerTestClass();

            Convention.CaseExecution
                      .SetUpTearDown(CaseSetUp, CaseTearDown);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass passed.",
                "SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                ".ctor",
                "CaseSetUp", "Pass", "CaseTearDown",
                "CaseSetUp", "Fail", "CaseTearDown",
                "Dispose");
        }

        public void ShouldShortCircuitInnerBehaviorAndTearDownByFailingCaseWhenConstructingPerCaseAndCaseSetUpThrows()
        {
            FailDuring("CaseSetUp");

            Convention.ClassExecution
                      .CreateInstancePerCase();

            Convention.CaseExecution
                      .SetUpTearDown(CaseSetUp, CaseTearDown);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: 'CaseSetUp' failed!",
                "SampleTestClass.Fail failed: 'CaseSetUp' failed!");

            output.ShouldHaveLifecycle(
                ".ctor",
                "CaseSetUp",
                "Dispose",
                ".ctor",
                "CaseSetUp",
                "Dispose");
        }

        public void ShouldShortCircuitInnerBehaviorAndTearDownByFailingAllCasesWhenConstructingPerTestClassAndCaseSetUpThrows()
        {
            FailDuring("CaseSetUp");

            Convention.ClassExecution
                      .CreateInstancePerTestClass();

            Convention.CaseExecution
                      .SetUpTearDown(CaseSetUp, CaseTearDown);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: 'CaseSetUp' failed!",
                "SampleTestClass.Fail failed: 'CaseSetUp' failed!");

            output.ShouldHaveLifecycle(
                ".ctor",
                "CaseSetUp",
                "CaseSetUp",
                "Dispose");
        }

        public void ShouldFailCaseWhenConstructingPerCaseAndCaseTearDownThrows()
        {
            FailDuring("CaseTearDown");

            Convention.ClassExecution
                      .CreateInstancePerCase();

            Convention.CaseExecution
                      .SetUpTearDown(CaseSetUp, CaseTearDown);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: 'CaseTearDown' failed!",
                "SampleTestClass.Fail failed: 'Fail' failed!" + Environment.NewLine +
                "    Secondary Failure: 'CaseTearDown' failed!");

            output.ShouldHaveLifecycle(
                ".ctor",
                "CaseSetUp", "Pass", "CaseTearDown",
                "Dispose",
                ".ctor",
                "CaseSetUp", "Fail", "CaseTearDown",
                "Dispose");
        }

        public void ShouldFailAllCasesWhenConstructingPerTestClassAndCaseTearDownThrows()
        {
            FailDuring("CaseTearDown");

            Convention.ClassExecution
                      .CreateInstancePerTestClass();

            Convention.CaseExecution
                      .SetUpTearDown(CaseSetUp, CaseTearDown);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: 'CaseTearDown' failed!",
                "SampleTestClass.Fail failed: 'Fail' failed!" + Environment.NewLine +
                "    Secondary Failure: 'CaseTearDown' failed!");

            output.ShouldHaveLifecycle(
                ".ctor",
                "CaseSetUp", "Pass", "CaseTearDown",
                "CaseSetUp", "Fail", "CaseTearDown",
                "Dispose");
        }
    }
}