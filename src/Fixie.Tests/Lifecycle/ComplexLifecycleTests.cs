namespace Fixie.Tests.Lifecycle
{
    using static System.Environment;

    public class ComplexLifecycleTests : BaseLifecycleTests
    {
        public void ShouldIncludeAllTearDownAndDisposalExceptionsInResultWhenConstructingPerCase()
        {
            FailDuring("ClassTearDown", "FixtureTearDown", "CaseTearDown", "Dispose");

            Convention.ClassExecution
                      .CreateInstancePerCase()
                      .Wrap<ClassSetUpTearDown>();

            Convention.FixtureExecution
                      .Wrap<FixtureSetUpTearDown>();

            Convention.CaseExecution
                      .Wrap<CaseSetUpTearDown>();

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: 'CaseTearDown' failed!" + NewLine +
	            "    Secondary Failure: 'FixtureTearDown' failed!" + NewLine +
                "    Secondary Failure: 'Dispose' failed!" + NewLine +
                "    Secondary Failure: 'ClassTearDown' failed!",
                "SampleTestClass.Fail failed: 'Fail' failed!" + NewLine +
	            "    Secondary Failure: 'CaseTearDown' failed!" + NewLine +
	            "    Secondary Failure: 'FixtureTearDown' failed!" + NewLine +
                "    Secondary Failure: 'Dispose' failed!" + NewLine +
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

            Convention.FixtureExecution
                      .Wrap<FixtureSetUpTearDown>();

            Convention.CaseExecution
                      .Wrap<CaseSetUpTearDown>();

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: 'CaseTearDown' failed!" + NewLine +
                "    Secondary Failure: 'FixtureTearDown' failed!" + NewLine +
                "    Secondary Failure: 'Dispose' failed!" + NewLine +
                "    Secondary Failure: 'ClassTearDown' failed!",
                "SampleTestClass.Fail failed: 'Fail' failed!" + NewLine +
                "    Secondary Failure: 'CaseTearDown' failed!" + NewLine +
                "    Secondary Failure: 'FixtureTearDown' failed!" + NewLine +
                "    Secondary Failure: 'Dispose' failed!" + NewLine +
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