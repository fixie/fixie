using System;
using Fixie.Behaviors;

namespace Fixie.Tests.Lifecycle
{
    public class InstanceLifecycleTests : LifecycleTests
    {
        class Inner : InstanceBehavior
        {
            public void Execute(InstanceExecution instanceExecution, Action next)
            {
                Console.WriteLine("Inner Before");
                next();
                Console.WriteLine("Inner After");
            }
        }

        class Outer : InstanceBehavior
        {
            public void Execute(InstanceExecution instanceExecution, Action next)
            {
                Console.WriteLine("Outer Before");
                next();
                Console.WriteLine("Outer After");
            }
        }

        class DoNothing : InstanceBehavior
        {
            public void Execute(InstanceExecution instanceExecution, Action next)
            {
                //Behavior chooses not to invoke next().
                //Since the cases are never invoked, they don't
                //have the chance to throw exceptions, resulting
                //in all 'passing'.
            }
        }

        class ThrowException : InstanceBehavior
        {
            public void Execute(InstanceExecution instanceExecution, Action next)
            {
                Console.WriteLine("Unsafe instance execution behavior");
                throw new Exception("Unsafe instance execution behavior threw!");
            }
        }

        class ThrowPreservedException : InstanceBehavior
        {
            public void Execute(InstanceExecution instanceExecution, Action next)
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
                      .Wrap<InstanceSetUpTearDown>();

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

        public void ShouldAllowWrappingInstanceWithSetUpTearDownBehaviorsWhenConstructingPerClass()
        {
            Convention.ClassExecution
                      .CreateInstancePerClass();

            Convention.InstanceExecution
                      .Wrap<InstanceSetUpTearDown>();

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
                      .Wrap<InstanceSetUpTearDown>();

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

        public void ShouldShortCircuitInnerBehaviorAndTearDownByFailingAllCasesWhenConstructingPerClassAndInstanceSetUpThrows()
        {
            FailDuring("InstanceSetUp");

            Convention.ClassExecution
                      .CreateInstancePerClass();

            Convention.InstanceExecution
                      .Wrap<InstanceSetUpTearDown>();

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
                      .Wrap<InstanceSetUpTearDown>();

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

        public void ShouldFailAllCasesWhenConstructingPerClassAndInstanceTearDownThrows()
        {
            FailDuring("InstanceTearDown");

            Convention.ClassExecution
                      .CreateInstancePerClass();

            Convention.InstanceExecution
                      .Wrap<InstanceSetUpTearDown>();

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
    }
}