using System;
using System.Collections.Generic;
using System.Reflection;
using Should;

namespace Fixie.Tests.Lifecycle
{
    public class ConstructionTests : LifecycleTests
    {
        public void ShouldConstructPerCaseByDefault()
        {
            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass passed",
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
                "SampleTestClass.Pass passed",
                "SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                ".ctor", "Pass", "Dispose",
                ".ctor", "Fail", "Dispose");
        }

        public void ShouldAllowConstructingPerClass()
        {
            Convention.ClassExecution
                      .CreateInstancePerClass();

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass passed",
                "SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                ".ctor", "Pass", "Fail", "Dispose");
        }

        public void ShouldAllowConstructingPerCaseUsingCustomFactory()
        {
            Convention.ClassExecution
                      .CreateInstancePerCase()
                      .UsingFactory(Factory);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass passed",
                "SampleTestClass.Fail failed: 'Fail' failed!");

            output.ShouldHaveLifecycle(
                "Factory", ".ctor", "Pass", "Dispose",
                "Factory", ".ctor", "Fail", "Dispose");
        }

        public void ShouldAllowConstructingPerClassUsingCustomFactory()
        {
            Convention.ClassExecution
                      .CreateInstancePerClass()
                      .UsingFactory(Factory);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass passed",
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

        public void ShouldFailAllCasesWhenConstructingPerClassAndConstructorThrows()
        {
            FailDuring(".ctor");

            Convention.ClassExecution
                      .CreateInstancePerClass();

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
                      .CreateInstancePerCase()
                      .UsingFactory(Factory);

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
                      .CreateInstancePerClass()
                      .UsingFactory(Factory);

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
                "SampleTestClass.Pass skipped",
                "SampleTestClass.Fail skipped");

            output.ShouldHaveLifecycle();
        }

        public void ShouldSkipConstructingPerClassWhenAllCasesSkipped()
        {
            Convention.ClassExecution
                      .CreateInstancePerClass();

            Convention.CaseExecution
                      .Skip(x => true);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass skipped",
                "SampleTestClass.Fail skipped");

            output.ShouldHaveLifecycle();
        }

        public void ShouldSkipConstructingPerCaseUsingCustomFactoryWhenAllCasesSkipped()
        {
            Convention.ClassExecution
                      .CreateInstancePerCase()
                      .UsingFactory(Factory);

            Convention.CaseExecution
                      .Skip(x => true);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass skipped",
                "SampleTestClass.Fail skipped");

            output.ShouldHaveLifecycle();
        }

        public void ShouldSkipConstructingPerClassUsingCustomFactoryWhenAllCasesSkipped()
        {
            Convention.ClassExecution
                      .CreateInstancePerClass()
                      .UsingFactory(Factory);

            Convention.CaseExecution
                      .Skip(x => true);

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass skipped",
                "SampleTestClass.Fail skipped");

            output.ShouldHaveLifecycle();
        }

        public void ShouldSkipConstructingPerCaseWhenAllCasesFailCustomParameterGeneration()
        {
            Convention.ClassExecution
                      .CreateInstancePerCase();

            Convention.Parameters.Add<BuggyParameterSource>();

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: Exception thrown while attempting to yield input parameters for method: Pass",
                "SampleTestClass.Fail failed: Exception thrown while attempting to yield input parameters for method: Fail");

            output.ShouldHaveLifecycle();
        }

        public void ShouldSkipConstructingPerClassWhenAllCasesFailCustomParameterGeneration()
        {
            Convention.ClassExecution
                      .CreateInstancePerClass();

            Convention.Parameters.Add<BuggyParameterSource>();

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: Exception thrown while attempting to yield input parameters for method: Pass",
                "SampleTestClass.Fail failed: Exception thrown while attempting to yield input parameters for method: Fail");

            output.ShouldHaveLifecycle();
        }

        public void ShouldSkipConstructingPerCaseUsingCustomFactoryWhenAllCasesFailCustomParameterGeneration()
        {
            Convention.ClassExecution
                      .CreateInstancePerCase()
                      .UsingFactory(Factory);

            Convention.Parameters.Add<BuggyParameterSource>();

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: Exception thrown while attempting to yield input parameters for method: Pass",
                "SampleTestClass.Fail failed: Exception thrown while attempting to yield input parameters for method: Fail");

            output.ShouldHaveLifecycle();
        }

        public void ShouldSkipConstructingPerClassUsingCustomFactoryWhenAllCasesFailCustomParameterGeneration()
        {
            Convention.ClassExecution
                      .CreateInstancePerClass()
                      .UsingFactory(Factory);

            Convention.Parameters.Add<BuggyParameterSource>();

            var output = Run();

            output.ShouldHaveResults(
                "SampleTestClass.Pass failed: Exception thrown while attempting to yield input parameters for method: Pass",
                "SampleTestClass.Fail failed: Exception thrown while attempting to yield input parameters for method: Fail");

            output.ShouldHaveLifecycle();
        }

        static object Factory(Type testClass)
        {
            WhereAmI();
            testClass.ShouldEqual(typeof(SampleTestClass));
            return new SampleTestClass();
        }

        class BuggyParameterSource : ParameterSource
        {
            public IEnumerable<object[]> GetParameters(MethodInfo method)
            {
                throw new Exception("Exception thrown while attempting to yield input parameters for method: " + method.Name);
            }
        }
    }
}