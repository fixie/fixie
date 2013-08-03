using System;

namespace Fixie.Tests.Lifecycle
{
    public class ComplexLifecycleTests : LifecycleTests
    {
        public void ShouldPerformCompleteLifecyclePerCaseWhenConstructingPerCase()
        {
            Convention.ClassExecution
                      .CreateInstancePerCase()
                      .SetUpTearDown(TypeSetUp, TypeTearDown);

            Convention.InstanceExecution
                      .SetUpTearDown(InstanceSetUp, InstanceTearDown);

            Convention.CaseExecution
                      .SetUpTearDown(CaseSetUp, CaseTearDown);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass passed.",
                "SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                "TypeSetUp",

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

                "TypeTearDown");
        }

        public void ShouldPerformCompleteLifecyclePerTestClassWhenConstructingPerTestClass()
        {
            Convention.ClassExecution
                      .CreateInstancePerTestClass()
                      .SetUpTearDown(TypeSetUp, TypeTearDown);

            Convention.InstanceExecution
                      .SetUpTearDown(InstanceSetUp, InstanceTearDown);

            Convention.CaseExecution
                      .SetUpTearDown(CaseSetUp, CaseTearDown);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass passed.",
                "SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                "TypeSetUp",
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
                "TypeTearDown");
        }

        public void ShouldIncludeAllTearDownAndDisposalExceptionsInResultWhenConstructingPerCase()
        {
            FailDuring("TypeTearDown", "InstanceTearDown", "CaseTearDown", "Dispose");

            Convention.ClassExecution
                      .CreateInstancePerCase()
                      .SetUpTearDown(TypeSetUp, TypeTearDown);

            Convention.InstanceExecution
                      .SetUpTearDown(InstanceSetUp, InstanceTearDown);

            Convention.CaseExecution
                      .SetUpTearDown(CaseSetUp, CaseTearDown);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: 'CaseTearDown' failed!" + Environment.NewLine +
	            "    Secondary Failure: 'InstanceTearDown' failed!" + Environment.NewLine +
                "    Secondary Failure: 'Dispose' failed!" + Environment.NewLine +
	            "    Secondary Failure: 'TypeTearDown' failed!",
                "SampleTestClass.Fail failed: 'Fail' failed!" + Environment.NewLine +
	            "    Secondary Failure: 'CaseTearDown' failed!" + Environment.NewLine +
	            "    Secondary Failure: 'InstanceTearDown' failed!" + Environment.NewLine +
                "    Secondary Failure: 'Dispose' failed!" + Environment.NewLine +
	            "    Secondary Failure: 'TypeTearDown' failed!");

            output.ShouldHaveLifecycle(
                "TypeSetUp",

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

                "TypeTearDown");
        }

        public void ShouldIncludeAllTearDownAndDisposalExceptionsInResultWhenConstructingPerTestClass()
        {
            FailDuring("TypeTearDown", "InstanceTearDown", "CaseTearDown", "Dispose");

            Convention.ClassExecution
                      .CreateInstancePerTestClass()
                      .SetUpTearDown(TypeSetUp, TypeTearDown);

            Convention.InstanceExecution
                      .SetUpTearDown(InstanceSetUp, InstanceTearDown);

            Convention.CaseExecution
                      .SetUpTearDown(CaseSetUp, CaseTearDown);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: 'CaseTearDown' failed!" + Environment.NewLine +
                "    Secondary Failure: 'InstanceTearDown' failed!" + Environment.NewLine +
                "    Secondary Failure: 'Dispose' failed!" + Environment.NewLine +
                "    Secondary Failure: 'TypeTearDown' failed!",
                "SampleTestClass.Fail failed: 'Fail' failed!" + Environment.NewLine +
                "    Secondary Failure: 'CaseTearDown' failed!" + Environment.NewLine +
                "    Secondary Failure: 'InstanceTearDown' failed!" + Environment.NewLine +
                "    Secondary Failure: 'Dispose' failed!" + Environment.NewLine +
                "    Secondary Failure: 'TypeTearDown' failed!");

            output.ShouldHaveLifecycle(
                "TypeSetUp",
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
                "TypeTearDown");
        }
    }
}