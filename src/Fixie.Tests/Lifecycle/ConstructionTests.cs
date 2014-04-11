using System;
using Fixie.Conventions;
using Should;

namespace Fixie.Tests.Lifecycle
{
    public class ConstructionTests : LifecycleTests
    {
        public void ShouldConstructPerCaseByDefault()
        {
            Expect(ConstructionFrequency.CreateInstancePerCase);

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

            Expect(ConstructionFrequency.CreateInstancePerCase);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass passed.",
                "SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                ".ctor", "Pass", "Dispose",
                ".ctor", "Fail", "Dispose");
        }

        public void ShouldAllowConstructingPerClass()
        {
            Convention.ClassExecution
                      .CreateInstancePerClass();

            Expect(ConstructionFrequency.CreateInstancePerClass);

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

            Expect(ConstructionFrequency.CreateInstancePerCase);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass passed.",
                "SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                "Factory", ".ctor", "Pass", "Dispose",
                "Factory", ".ctor", "Fail", "Dispose");
        }

        public void ShouldAllowConstructingPerClassUsingCustomFactory()
        {
            Convention.ClassExecution
                      .CreateInstancePerClass(Factory);

            Expect(ConstructionFrequency.CreateInstancePerClass);

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

            Expect(ConstructionFrequency.CreateInstancePerCase);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: '.ctor' failed!",
                "SampleTestClass.Fail failed: '.ctor' failed!");

            output.ShouldHaveLifecycle(
                ".ctor",
                ".ctor");
        }

        public void ShouldFailAllCasesWhenConstructingPerClassAndConstructorThrows()
        {
            FailDuring(".ctor");

            Convention.ClassExecution
                      .CreateInstancePerClass();

            Expect(ConstructionFrequency.CreateInstancePerClass);

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

            Expect(ConstructionFrequency.CreateInstancePerCase);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: 'Factory' failed!",
                "SampleTestClass.Fail failed: 'Factory' failed!");

            output.ShouldHaveLifecycle(
                "Factory",
                "Factory");
        }

        public void ShouldFailAllCasesWhenConstructingPerClassAndCustomFactoryThrows()
        {
            FailDuring("Factory");

            Convention.ClassExecution
                      .CreateInstancePerClass(Factory);

            Expect(ConstructionFrequency.CreateInstancePerClass);

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

            Expect(ConstructionFrequency.CreateInstancePerCase);

            Convention.CaseExecution
                      .Skip(x => true);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass skipped.",
                "SampleTestClass.Fail skipped.");

            output.ShouldHaveLifecycle();
        }

        public void ShouldSkipConstructingPerClassWhenAllCasesSkipped()
        {
            Convention.ClassExecution
                      .CreateInstancePerClass();

            Expect(ConstructionFrequency.CreateInstancePerClass);

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

            Expect(ConstructionFrequency.CreateInstancePerCase);

            Convention.CaseExecution
                      .Skip(x => true);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass skipped.",
                "SampleTestClass.Fail skipped.");

            output.ShouldHaveLifecycle();
        }

        public void ShouldSkipConstructingPerClassUsingCustomFactoryWhenAllCasesSkipped()
        {
            Convention.ClassExecution
                      .CreateInstancePerClass(Factory);

            Expect(ConstructionFrequency.CreateInstancePerClass);

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

        void Expect(ConstructionFrequency expected)
        {
            Convention.ClassExecution.ConstructionFrequency.ShouldEqual(expected);
        }
    }
}