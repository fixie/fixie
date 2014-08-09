using System;

namespace Fixie.Tests.Lifecycle
{
    public class ComplexLifecycleTests : LifecycleTests
    {
        public void ShouldPerformCompleteLifecyclePerCaseWhenConstructingPerCase()
        {
            Convention.ClassExecution
                      .CreateInstancePerCase()
                      .Wrap<ClassSetUpTearDown>();

            Convention.InstanceExecution
                      .Wrap<FixtureSetUpTearDown>();

            Convention.CaseExecution
                      .Wrap<CaseSetUpTearDown>();

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass passed.",
                "SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                "ClassSetUp",

                ".ctor",
                "FixtureSetUp",
                "CaseSetUp",
                "Pass",
                "CaseTearDown",
                "FixtureTearDown",
                "Dispose",

                ".ctor",
                "FixtureSetUp",
                "CaseSetUp",
                "Fail",
                "CaseTearDown",
                "FixtureTearDown",
                "Dispose",

                "ClassTearDown");
        }

        public void ShouldPerformCompleteLifecyclePerClassWhenConstructingPerClass()
        {
            Convention.ClassExecution
                      .CreateInstancePerClass()
                      .Wrap<ClassSetUpTearDown>();

            Convention.InstanceExecution
                      .Wrap<FixtureSetUpTearDown>();

            Convention.CaseExecution
                      .Wrap<CaseSetUpTearDown>();

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass passed.",
                "SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                "ClassSetUp",
                ".ctor",

                "FixtureSetUp",
                "CaseSetUp",
                "Pass",
                "CaseTearDown",

                "CaseSetUp",
                "Fail",
                "CaseTearDown",
                "FixtureTearDown",

                "Dispose",
                "ClassTearDown");
        }

        public void ShouldIncludeAllTearDownAndDisposalExceptionsInResultWhenConstructingPerCase()
        {
            FailDuring("ClassTearDown", "FixtureTearDown", "CaseTearDown", "Dispose");

            Convention.ClassExecution
                      .CreateInstancePerCase()
                      .Wrap<ClassSetUpTearDown>();

            Convention.InstanceExecution
                      .Wrap<FixtureSetUpTearDown>();

            Convention.CaseExecution
                      .Wrap<CaseSetUpTearDown>();

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: 'CaseTearDown' failed!" + Environment.NewLine +
	            "    Secondary Failure: 'FixtureTearDown' failed!" + Environment.NewLine +
                "    Secondary Failure: 'Dispose' failed!" + Environment.NewLine +
                "    Secondary Failure: 'ClassTearDown' failed!",
                "SampleTestClass.Fail failed: 'Fail' failed!" + Environment.NewLine +
	            "    Secondary Failure: 'CaseTearDown' failed!" + Environment.NewLine +
	            "    Secondary Failure: 'FixtureTearDown' failed!" + Environment.NewLine +
                "    Secondary Failure: 'Dispose' failed!" + Environment.NewLine +
                "    Secondary Failure: 'ClassTearDown' failed!");

            output.ShouldHaveLifecycle(
                "ClassSetUp",

                ".ctor",
                "FixtureSetUp",
                "CaseSetUp",
                "Pass",
                "CaseTearDown",
                "FixtureTearDown",
                "Dispose",

                ".ctor",
                "FixtureSetUp",
                "CaseSetUp",
                "Fail",
                "CaseTearDown",
                "FixtureTearDown",
                "Dispose",

                "ClassTearDown");
        }

        public void ShouldIncludeAllTearDownAndDisposalExceptionsInResultWhenConstructingPerClass()
        {
            FailDuring("ClassTearDown", "FixtureTearDown", "CaseTearDown", "Dispose");

            Convention.ClassExecution
                      .CreateInstancePerClass()
                      .Wrap<ClassSetUpTearDown>();

            Convention.InstanceExecution
                      .Wrap<FixtureSetUpTearDown>();

            Convention.CaseExecution
                      .Wrap<CaseSetUpTearDown>();

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: 'CaseTearDown' failed!" + Environment.NewLine +
                "    Secondary Failure: 'FixtureTearDown' failed!" + Environment.NewLine +
                "    Secondary Failure: 'Dispose' failed!" + Environment.NewLine +
                "    Secondary Failure: 'ClassTearDown' failed!",
                "SampleTestClass.Fail failed: 'Fail' failed!" + Environment.NewLine +
                "    Secondary Failure: 'CaseTearDown' failed!" + Environment.NewLine +
                "    Secondary Failure: 'FixtureTearDown' failed!" + Environment.NewLine +
                "    Secondary Failure: 'Dispose' failed!" + Environment.NewLine +
                "    Secondary Failure: 'ClassTearDown' failed!");

            output.ShouldHaveLifecycle(
                "ClassSetUp",
                ".ctor",

                "FixtureSetUp",
                "CaseSetUp",
                "Pass",
                "CaseTearDown",

                "CaseSetUp",
                "Fail",
                "CaseTearDown",
                "FixtureTearDown",

                "Dispose",
                "ClassTearDown");
        }
    }
}