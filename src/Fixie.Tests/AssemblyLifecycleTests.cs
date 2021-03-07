namespace Fixie.Tests
{
    using System;
    using System.Threading.Tasks;
    using Assertions;
    using Fixie.Internal;

    public class AssemblyLifecycleTests : InstrumentedExecutionTests
    {
        class FirstTestClass
        {
            public void Fail()
            {
                WhereAmI();
                throw new FailureException();
            }
        }

        class SecondTestClass
        {
            public void Pass() => WhereAmI();
        }

        class CustomExecution : Execution
        {
            public async Task RunAsync(TestAssembly testAssembly)
            {
                AssemblySetUp();

                foreach (var testClass in testAssembly.TestClasses)
                    foreach (var test in testClass.Tests)
                        await test.RunAsync();

                AssemblyTearDown();
            }

            static void AssemblySetUp() => WhereAmI();
            static void AssemblyTearDown() => WhereAmI();
        }

        static readonly Type[] TestClasses = {typeof(FirstTestClass), typeof(SecondTestClass)};

        public async Task ShouldPerformNoAssemblyLevelBehaviorsByDefault()
        {
            var output = await RunAsync(TestClasses, new DefaultExecution());

            output.ShouldHaveResults(
                "FirstTestClass.Fail failed: 'Fail' failed!",
                "SecondTestClass.Pass passed");

            output.ShouldHaveLifecycle(
                "Fail", "Pass");
        }

        public async Task ShouldPerformOptionalAssemblyLevelBehaviors()
        {
            var output = await RunSampleAsync();

            output.ShouldHaveResults(
                "FirstTestClass.Fail failed: 'Fail' failed!",
                "SecondTestClass.Pass passed");

            output.ShouldHaveLifecycle(
                "AssemblySetUp",
                "Fail", "Pass",
                "AssemblyTearDown");
        }

        public async Task ShouldFailAllTestsWithoutHidingPrimarySkipResultsWhenAssemblySetUpThrows()
        {
            FailDuring("AssemblySetUp");

            var output = await RunSampleAsync();

            output.ShouldHaveResults(
                "FirstTestClass.Fail failed: 'AssemblySetUp' failed!",
                "FirstTestClass.Fail skipped: This test did not run.",

                "SecondTestClass.Pass failed: 'AssemblySetUp' failed!",
                "SecondTestClass.Pass skipped: This test did not run.");

            output.ShouldHaveLifecycle("AssemblySetUp");
        }

        public async Task ShouldFailAllTestsWithoutHidingPrimaryCaseResultsWhenAssemblyTearDownThrows()
        {
            FailDuring("AssemblyTearDown");

            var output = await RunSampleAsync();

            output.ShouldHaveResults(
                "FirstTestClass.Fail failed: 'Fail' failed!",
                "SecondTestClass.Pass passed",

                "FirstTestClass.Fail failed: 'AssemblyTearDown' failed!",
                "SecondTestClass.Pass failed: 'AssemblyTearDown' failed!");

            output.ShouldHaveLifecycle(
                "AssemblySetUp",
                "Fail", "Pass",
                "AssemblyTearDown");
        }

        async Task<Output> RunSampleAsync() => await RunAsync(TestClasses, new CustomExecution());
    }
}
