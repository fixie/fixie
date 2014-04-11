using System;

namespace Fixie.Tests.Lifecycle
{
    public class ComplexLifecycleTests : LifecycleTests
    {
        public void ShouldPerformCompleteLifecyclePerCaseWhenConstructingPerCase()
        {
            Convention.ClassExecution
                      .CreateInstancePerCase()
                      .SetUpTearDown(ClassSetUp, ClassTearDown);

            Convention.InstanceExecution
                      .SetUpTearDown(InstanceSetUp, InstanceTearDown);

            Convention.CaseExecution
                      .SetUpTearDown(CaseSetUp, CaseTearDown);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass passed.",
                "SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                "ClassSetUp",

                ".ctor",
                "InstanceSetUp",
                "CaseSetUp",
                "Pass",
                "CaseTearDown",
                "InstanceTearDown",
                "Dispose",

                ".ctor",
                "InstanceSetUp",
                "CaseSetUp",
                "Fail",
                "CaseTearDown",
                "InstanceTearDown",
                "Dispose",

                "ClassTearDown");
        }

        public void ShouldPerformCompleteLifecyclePerClassWhenConstructingPerClass()
        {
            Convention.ClassExecution
                      .CreateInstancePerClass()
                      .SetUpTearDown(ClassSetUp, ClassTearDown);

            Convention.InstanceExecution
                      .SetUpTearDown(InstanceSetUp, InstanceTearDown);

            Convention.CaseExecution
                      .SetUpTearDown(CaseSetUp, CaseTearDown);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass passed.",
                "SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                "ClassSetUp",
                ".ctor",

                "InstanceSetUp",
                "CaseSetUp",
                "Pass",
                "CaseTearDown",

                "CaseSetUp",
                "Fail",
                "CaseTearDown",
                "InstanceTearDown",

                "Dispose",
                "ClassTearDown");
        }

        public void ShouldIncludeAllTearDownAndDisposalExceptionsInResultWhenConstructingPerCase()
        {
            FailDuring("ClassTearDown", "InstanceTearDown", "CaseTearDown", "Dispose");

            Convention.ClassExecution
                      .CreateInstancePerCase()
                      .SetUpTearDown(ClassSetUp, ClassTearDown);

            Convention.InstanceExecution
                      .SetUpTearDown(InstanceSetUp, InstanceTearDown);

            Convention.CaseExecution
                      .SetUpTearDown(CaseSetUp, CaseTearDown);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: 'CaseTearDown' failed!" + Environment.NewLine +
	            "    Secondary Failure: 'InstanceTearDown' failed!" + Environment.NewLine +
                "    Secondary Failure: 'Dispose' failed!" + Environment.NewLine +
                "    Secondary Failure: 'ClassTearDown' failed!",
                "SampleTestClass.Fail failed: 'Fail' failed!" + Environment.NewLine +
	            "    Secondary Failure: 'CaseTearDown' failed!" + Environment.NewLine +
	            "    Secondary Failure: 'InstanceTearDown' failed!" + Environment.NewLine +
                "    Secondary Failure: 'Dispose' failed!" + Environment.NewLine +
                "    Secondary Failure: 'ClassTearDown' failed!");

            output.ShouldHaveLifecycle(
                "ClassSetUp",

                ".ctor",
                "InstanceSetUp",
                "CaseSetUp",
                "Pass",
                "CaseTearDown",
                "InstanceTearDown",
                "Dispose",

                ".ctor",
                "InstanceSetUp",
                "CaseSetUp",
                "Fail",
                "CaseTearDown",
                "InstanceTearDown",
                "Dispose",

                "ClassTearDown");
        }

        public void ShouldIncludeAllTearDownAndDisposalExceptionsInResultWhenConstructingPerClass()
        {
            FailDuring("ClassTearDown", "InstanceTearDown", "CaseTearDown", "Dispose");

            Convention.ClassExecution
                      .CreateInstancePerClass()
                      .SetUpTearDown(ClassSetUp, ClassTearDown);

            Convention.InstanceExecution
                      .SetUpTearDown(InstanceSetUp, InstanceTearDown);

            Convention.CaseExecution
                      .SetUpTearDown(CaseSetUp, CaseTearDown);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: 'CaseTearDown' failed!" + Environment.NewLine +
                "    Secondary Failure: 'InstanceTearDown' failed!" + Environment.NewLine +
                "    Secondary Failure: 'Dispose' failed!" + Environment.NewLine +
                "    Secondary Failure: 'ClassTearDown' failed!",
                "SampleTestClass.Fail failed: 'Fail' failed!" + Environment.NewLine +
                "    Secondary Failure: 'CaseTearDown' failed!" + Environment.NewLine +
                "    Secondary Failure: 'InstanceTearDown' failed!" + Environment.NewLine +
                "    Secondary Failure: 'Dispose' failed!" + Environment.NewLine +
                "    Secondary Failure: 'ClassTearDown' failed!");

            output.ShouldHaveLifecycle(
                "ClassSetUp",
                ".ctor",

                "InstanceSetUp",
                "CaseSetUp",
                "Pass",
                "CaseTearDown",

                "CaseSetUp",
                "Fail",
                "CaseTearDown",
                "InstanceTearDown",

                "Dispose",
                "ClassTearDown");
        }
    }
}