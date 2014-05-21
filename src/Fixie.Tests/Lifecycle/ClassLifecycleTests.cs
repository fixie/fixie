using System;

namespace Fixie.Tests.Lifecycle
{
    public class ClassLifecycleTests : LifecycleTests
    {
        public void ShouldAllowWrappingClassWithBehaviorsWhenConstructingPerCase()
        {
            Convention.ClassExecution
                      .CreateInstancePerCase()
                      .Wrap((classExecution, innerBehavior) =>
                      {
                          Console.WriteLine("Inner Before");
                          innerBehavior();
                          Console.WriteLine("Inner After");
                      })
                      .Wrap((classExecution, innerBehavior) =>
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

        public void ShouldAllowWrappingClassWithBehaviorsWhenConstructingPerClass()
        {
            Convention.ClassExecution
                      .CreateInstancePerClass()
                      .Wrap((classExecution, innerBehavior) =>
                      {
                          Console.WriteLine("Inner Before");
                          innerBehavior();
                          Console.WriteLine("Inner After");
                      })
                      .Wrap((classExecution, innerBehavior) =>
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

        public void ShouldAllowClassBehaviorsToShortCircuitInnerBehaviorWhenConstructingPerCase()
        {
            Convention.ClassExecution
                      .CreateInstancePerCase()
                      .Wrap((classExecution, innerBehavior) =>
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

        public void ShouldAllowClassBehaviorsToShortCircuitInnerBehaviorWhenConstructingPerClass()
        {
            Convention.ClassExecution
                      .CreateInstancePerClass()
                      .Wrap((classExecution, innerBehavior) =>
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

        public void ShouldFailCaseWhenConstructingPerCaseAndClassBehaviorThrows()
        {
            Convention.ClassExecution
                      .CreateInstancePerCase()
                      .Wrap((classExecution, innerBehavior) =>
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

        public void ShouldFailAllCasesWhenConstructingPerClassAndClassBehaviorThrows()
        {
            Convention.ClassExecution
                      .CreateInstancePerClass()
                      .Wrap((classExecution, innerBehavior) =>
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

        public void ShouldFailCaseWithOriginalExceptionWhenConstructingPerCaseAndClassBehaviorThrowsPreservedException()
        {
            Convention.ClassExecution
                      .CreateInstancePerCase()
                      .Wrap((classExecution, innerBehavior) =>
                      {
                          Console.WriteLine("Unsafe class execution behavior");
                          try
                          {
                              throw new Exception("Unsafe class execution behavior threw!");
                          }
                          catch (Exception originalException)
                          {
                              throw new PreservedException(originalException);
                          }
                      });

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: Unsafe class execution behavior threw!",
                "SampleTestClass.Fail failed: Unsafe class execution behavior threw!");

            output.ShouldHaveLifecycle("Unsafe class execution behavior");
        }

        public void ShouldFailAllCasesWithOriginalExceptionWhenConstructingPerClassAndClassBehaviorThrowsPreservedException()
        {
            Convention.ClassExecution
                      .CreateInstancePerClass()
                      .Wrap((classExecution, innerBehavior) =>
                      {
                          Console.WriteLine("Unsafe class execution behavior");
                          try
                          {
                              throw new Exception("Unsafe class execution behavior threw!");
                          }
                          catch (Exception originalException)
                          {
                              throw new PreservedException(originalException);
                          }
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
                      .SetUpTearDown(ClassSetUp, ClassTearDown);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass passed.",
                "SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                "ClassSetUp",
                ".ctor", "Pass", "Dispose",
                ".ctor", "Fail", "Dispose",
                "ClassTearDown");
        }

        public void ShouldAllowWrappingClassWithSetUpTearDownBehaviorsWhenConstructingPerClass()
        {
            Convention.ClassExecution
                      .CreateInstancePerClass()
                      .SetUpTearDown(ClassSetUp, ClassTearDown);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass passed.",
                "SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                "ClassSetUp",
                ".ctor", "Pass", "Fail", "Dispose",
                "ClassTearDown");
        }

        public void ShouldShortCircuitInnerBehaviorAndTearDownByFailingCaseWhenConstructingPerCaseAndClassSetUpThrows()
        {
            FailDuring("ClassSetUp");

            Convention.ClassExecution
                      .CreateInstancePerCase()
                      .SetUpTearDown(ClassSetUp, ClassTearDown);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: 'ClassSetUp' failed!",
                "SampleTestClass.Fail failed: 'ClassSetUp' failed!");

            output.ShouldHaveLifecycle(
                "ClassSetUp");
        }

        public void ShouldShortCircuitInnerBehaviorAndTearDownByFailingAllCasesWhenConstructingPerClassAndClassSetUpThrows()
        {
            FailDuring("ClassSetUp");

            Convention.ClassExecution
                      .CreateInstancePerClass()
                      .SetUpTearDown(ClassSetUp, ClassTearDown);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: 'ClassSetUp' failed!",
                "SampleTestClass.Fail failed: 'ClassSetUp' failed!");

            output.ShouldHaveLifecycle(
                "ClassSetUp");
        }

        public void ShouldFailCaseWhenConstructingPerCaseAndClassTearDownThrows()
        {
            FailDuring("ClassTearDown");

            Convention.ClassExecution
                      .CreateInstancePerCase()
                      .SetUpTearDown(ClassSetUp, ClassTearDown);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: 'ClassTearDown' failed!",
                "SampleTestClass.Fail failed: 'Fail' failed!" + Environment.NewLine +
                "    Secondary Failure: 'ClassTearDown' failed!");

            output.ShouldHaveLifecycle(
                "ClassSetUp",
                ".ctor", "Pass", "Dispose",
                ".ctor", "Fail", "Dispose",
                "ClassTearDown");
        }

        public void ShouldFailAllCasesWhenConstructingPerClassAndClassTearDownThrows()
        {
            FailDuring("ClassTearDown");

            Convention.ClassExecution
                      .CreateInstancePerClass()
                      .SetUpTearDown(ClassSetUp, ClassTearDown);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: 'ClassTearDown' failed!",
                "SampleTestClass.Fail failed: 'Fail' failed!" + Environment.NewLine +
                "    Secondary Failure: 'ClassTearDown' failed!");

            output.ShouldHaveLifecycle(
                "ClassSetUp",
                ".ctor", "Pass", "Fail", "Dispose",
                "ClassTearDown");
        }

        public void ShouldAllowWrappingClassWithSetUpBehaviorWhenConstructingPerCase()
        {
            Convention.ClassExecution
                      .CreateInstancePerCase()
                      .SetUp(ClassSetUp);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass passed.",
                "SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                "ClassSetUp",
                ".ctor", "Pass", "Dispose",
                ".ctor", "Fail", "Dispose");
        }

        public void ShouldAllowWrappingClassWithSetUpBehaviorWhenConstructingPerClass()
        {
            Convention.ClassExecution
                      .CreateInstancePerClass()
                      .SetUp(ClassSetUp);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass passed.",
                "SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                "ClassSetUp",
                ".ctor", "Pass", "Fail", "Dispose");
        }

        public void ShouldShortCircuitInnerBehaviorByFailingCaseWhenConstructingPerCaseAndClassSetUpThrows()
        {
            FailDuring("ClassSetUp");

            Convention.ClassExecution
                      .CreateInstancePerCase()
                      .SetUp(ClassSetUp);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: 'ClassSetUp' failed!",
                "SampleTestClass.Fail failed: 'ClassSetUp' failed!");

            output.ShouldHaveLifecycle(
                "ClassSetUp");
        }

        public void ShouldShortCircuitInnerBehaviorByFailingAllCasesWhenConstructingPerClassAndClassSetUpThrows()
        {
            FailDuring("ClassSetUp");

            Convention.ClassExecution
                      .CreateInstancePerClass()
                      .SetUp(ClassSetUp);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: 'ClassSetUp' failed!",
                "SampleTestClass.Fail failed: 'ClassSetUp' failed!");

            output.ShouldHaveLifecycle(
                "ClassSetUp");
        }
    }
}