using System;
using Should;

namespace Fixie.Tests.Lifecycle
{
    public class ConstructionTests : LifecycleTests
    {
        public void ShouldConstructPerCaseByDefault()
        {
            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass passed.",
                "SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                ".ctor", "Pass", "Dispose",
                ".ctor", "Fail", "Dispose");
        }

        public void ShouldAllowConstructingPerCaseExplicitly()
        {
            Convention.ClassExecution
                      .CreateInstancePerCase();

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass passed.",
                "SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                ".ctor", "Pass", "Dispose",
                ".ctor", "Fail", "Dispose");
        }

        public void ShouldAllowConstructingPerTestClass()
        {
            Convention.ClassExecution
                      .CreateInstancePerTestClass();

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass passed.",
                "SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                ".ctor", "Pass", "Fail", "Dispose");
        }

        public void ShouldAllowConstructingPerCaseUsingCustomFactory()
        {
            Convention.ClassExecution
                      .CreateInstancePerCase(Factory);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass passed.",
                "SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                "Factory", ".ctor", "Pass", "Dispose",
                "Factory", ".ctor", "Fail", "Dispose");
        }

        public void ShouldAllowConstructingPerTestClassUsingCustomFactory()
        {
            Convention.ClassExecution
                      .CreateInstancePerTestClass(Factory);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass passed.",
                "SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                "Factory", ".ctor", "Pass", "Fail", "Dispose");
        }

        public void ShouldFailCaseWhenConstructingPerCaseAndConstructorThrows()
        {
            FailDuring(".ctor");

            Convention.ClassExecution
                      .CreateInstancePerCase();

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: '.ctor' failed!",
                "SampleTestClass.Fail failed: '.ctor' failed!");

            output.ShouldHaveLifecycle(
                ".ctor",
                ".ctor");
        }

        public void ShouldFailAllCasesWhenConstructingPerTestClassAndConstructorThrows()
        {
            FailDuring(".ctor");

            Convention.ClassExecution
                      .CreateInstancePerTestClass();

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: '.ctor' failed!",
                "SampleTestClass.Fail failed: '.ctor' failed!");

            output.ShouldHaveLifecycle(
                ".ctor");
        }

        public void ShouldFailCaseWhenConstructingPerCaseAndCustomFactoryThrows()
        {
            FailDuring("Factory");

            Convention.ClassExecution
                      .CreateInstancePerCase(Factory);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: 'Factory' failed!",
                "SampleTestClass.Fail failed: 'Factory' failed!");

            output.ShouldHaveLifecycle(
                "Factory",
                "Factory");
        }

        public void ShouldFailAllCasesWhenConstructingPerTestClassAndCustomFactoryThrows()
        {
            FailDuring("Factory");

            Convention.ClassExecution
                      .CreateInstancePerTestClass(Factory);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: 'Factory' failed!",
                "SampleTestClass.Fail failed: 'Factory' failed!");

            output.ShouldHaveLifecycle(
                "Factory");
        }

        public void ShouldSkipConstructingPerCaseWhenAllCasesSkipped()
        {
            Convention.ClassExecution
                      .CreateInstancePerCase();

            Convention.CaseExecution
                      .Skip(x => true);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass skipped.",
                "SampleTestClass.Fail skipped.");

            output.ShouldHaveLifecycle();
        }

        public void ShouldSkipConstructingPerTestClassWhenAllCasesSkipped()
        {
            Convention.ClassExecution
                      .CreateInstancePerTestClass();

            Convention.CaseExecution
                      .Skip(x => true);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass skipped.",
                "SampleTestClass.Fail skipped.");

            output.ShouldHaveLifecycle();
        }

        public void ShouldSkipConstructingPerCaseUsingCustomFactoryWhenAllCasesSkipped()
        {
            Convention.ClassExecution
                      .CreateInstancePerCase(Factory);

            Convention.CaseExecution
                      .Skip(x => true);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass skipped.",
                "SampleTestClass.Fail skipped.");

            output.ShouldHaveLifecycle();
        }

        public void ShouldSkipConstructingPerTestClassUsingCustomFactoryWhenAllCasesSkipped()
        {
            Convention.ClassExecution
                      .CreateInstancePerTestClass(Factory);

            Convention.CaseExecution
                      .Skip(x => true);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass skipped.",
                "SampleTestClass.Fail skipped.");

            output.ShouldHaveLifecycle();
        }

        static object Factory(Type testClass)
        {
            WhereAmI();
            testClass.ShouldEqual(typeof(SampleTestClass));
            return new SampleTestClass();
        }
    }
}