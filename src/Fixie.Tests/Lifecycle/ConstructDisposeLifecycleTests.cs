using System;
using Should;

namespace Fixie.Tests.Lifecycle
{
    public class ConstructDisposeLifecycleTests : LifecycleTests
    {
        public void ShouldCreateInstancePerCaseByDefault()
        {
            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass passed.",
                "SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                ".ctor", "Pass", "Dispose",
                ".ctor", "Fail", "Dispose");
        }

        public void ShouldAllowCreatingInstancePerCaseExplicitly()
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

        public void ShouldAllowCreatingInstancePerTestClass()
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

        public void ShouldAllowCreatingInstancePerCaseUsingCustomFactory()
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

        public void ShouldAllowCreatingInstancePerTestClassUsingCustomFactory()
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

        public void ShouldFailAllCasesWhenCreatingInstancePerCaseAndConstructorThrows()
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

        public void ShouldFailAllCasesWhenCreatingInstancePerTestClassAndConstructorThrows()
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

        public void ShouldFailAllCasesWhenCreatingInstancePerCaseAndCustomFactoryThrows()
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

        public void ShouldFailAllCasesWhenCreatingInstancePerTestClassAndCustomFactoryThrows()
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

        static object Factory(Type testClass)
        {
            WhereAmI();
            testClass.ShouldEqual(typeof(SampleTestClass));
            return new SampleTestClass();
        }
    }
}