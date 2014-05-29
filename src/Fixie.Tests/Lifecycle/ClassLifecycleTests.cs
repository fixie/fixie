using System;
using Fixie.Behaviors;

namespace Fixie.Tests.Lifecycle
{
    public class ClassLifecycleTests : LifecycleTests
    {
        class Inner : ClassBehavior
        {
            public void Execute(ClassExecution classExecution, Action next)
            {
                Console.WriteLine("Inner Before");
                next();
                Console.WriteLine("Inner After");
            }
        }

        class Outer : ClassBehavior
        {
            public void Execute(ClassExecution classExecution, Action next)
            {
                Console.WriteLine("Outer Before");
                next();
                Console.WriteLine("Outer After");
            }
        }

        class DoNothing : ClassBehavior
        {
            public void Execute(ClassExecution classExecution, Action next)
            {
                //Behavior chooses not to invoke next().
                //Since the test classes are never intantiated,
                //their cases don't have the chance to throw exceptions,
                //resulting in all 'passing'.
            }
        }

        class ThrowException : ClassBehavior
        {
            public void Execute(ClassExecution classExecution, Action next)
            {
                Console.WriteLine("Unsafe class execution behavior");
                throw new Exception("Unsafe class execution behavior threw!");
            }
        }

        class ThrowPreservedException : ClassBehavior
        {
            public void Execute(ClassExecution classExecution, Action next)
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
            }
        }

        public void ShouldAllowWrappingClassWithBehaviorsWhenConstructingPerCase()
        {
            Convention.ClassExecution
                      .CreateInstancePerCase()
                      .Wrap<Inner>()
                      .Wrap<Outer>();

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
                      .Wrap<Inner>()
                      .Wrap<Outer>();

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
                      .Wrap<DoNothing>();

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
                      .Wrap<DoNothing>();

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
                      .Wrap<ThrowException>();

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
                      .Wrap<ThrowException>();

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
                      .Wrap<ThrowPreservedException>();

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
                      .Wrap<ThrowPreservedException>();

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
                      .Wrap<ClassSetUpTearDown>();

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
                      .Wrap<ClassSetUpTearDown>();

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
                      .Wrap<ClassSetUpTearDown>();

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
                      .Wrap<ClassSetUpTearDown>();

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
                      .Wrap<ClassSetUpTearDown>();

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
                      .Wrap<ClassSetUpTearDown>();

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
    }
}