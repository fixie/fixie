using System;

namespace Fixie.Tests.Lifecycle
{
    public class CaseLifecycleTests : LifecycleTests
    {
        class Inner : CaseBehavior
        {
            public void Execute(CaseExecution caseExecution, Action next)
            {
                Console.WriteLine("Inner Before");
                next();
                Console.WriteLine("Inner After");
            }
        }

        class Outer : CaseBehavior
        {
            public void Execute(CaseExecution caseExecution, Action next)
            {
                Console.WriteLine("Outer Before");
                next();
                Console.WriteLine("Outer After");
            }
        }

        class DoNothing : CaseBehavior
        {
            public void Execute(CaseExecution caseExecution, Action next)
            {
                //Behavior chooses not to invoke next().
                //Since the cases are never invoked, they don't
                //have the chance to throw exceptions, resulting
                //in all 'passing'.
            }
        }

        class ThrowException : CaseBehavior
        {
            public void Execute(CaseExecution caseExecution, Action next)
            {
                Console.WriteLine("Unsafe case execution behavior");
                throw new Exception("Unsafe case execution behavior threw!");
            }
        }

        class ThrowPreservedException : CaseBehavior
        {
            public void Execute(CaseExecution caseExecution, Action next)
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
            }
        }

        public void ShouldAllowWrappingCaseWithBehaviorsWhenConstructingPerCase()
        {
            Convention.ClassExecution
                      .CreateInstancePerCase();

            Convention.CaseExecution
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

        public void ShouldAllowWrappingCaseWithBehaviorsWhenConstructingPerClass()
        {
            Convention.ClassExecution
                      .CreateInstancePerClass();

            Convention.CaseExecution
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
        
        public void ShouldAllowCaseBehaviorsToShortCircuitInnerBehaviorWhenConstructingPerClass()
        {
            Convention.ClassExecution
                      .CreateInstancePerClass();

            Convention.CaseExecution
                      .Wrap<DoNothing>();

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
                      .Wrap<ThrowException>();

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

        public void ShouldFailAllCasesWhenConstructingPerClassAndCaseBehaviorThrows()
        {
            Convention.ClassExecution
                      .CreateInstancePerClass();

            Convention.CaseExecution
                      .Wrap<ThrowException>();

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
                      .Wrap<ThrowPreservedException>();

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

        public void ShouldFailAllCasesWithOriginalExceptionWhenConstructingPerClassAndCaseBehaviorThrowsPreservedException()
        {
            Convention.ClassExecution
                      .CreateInstancePerClass();

            Convention.CaseExecution
                      .Wrap<ThrowPreservedException>();

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
                      .Wrap<CaseSetUpTearDown>();

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

        public void ShouldAllowWrappingCaseWithSetUpTearDownBehaviorsWhenConstructingPerClass()
        {
            Convention.ClassExecution
                      .CreateInstancePerClass();

            Convention.CaseExecution
                      .Wrap<CaseSetUpTearDown>();

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
                      .Wrap<CaseSetUpTearDown>();

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

        public void ShouldShortCircuitInnerBehaviorAndTearDownByFailingAllCasesWhenConstructingPerClassAndCaseSetUpThrows()
        {
            FailDuring("CaseSetUp");

            Convention.ClassExecution
                      .CreateInstancePerClass();

            Convention.CaseExecution
                      .Wrap<CaseSetUpTearDown>();

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
                      .Wrap<CaseSetUpTearDown>();

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

        public void ShouldFailAllCasesWhenConstructingPerClassAndCaseTearDownThrows()
        {
            FailDuring("CaseTearDown");

            Convention.ClassExecution
                      .CreateInstancePerClass();

            Convention.CaseExecution
                      .Wrap<CaseSetUpTearDown>();

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