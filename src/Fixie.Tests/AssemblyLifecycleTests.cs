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
            public Task StartAsync()
            {
                WhereAmI();
                return Task.CompletedTask;
            }

            public async Task RunAsync(TestClass testClass)
            {
                foreach (var test in testClass.Tests)
                    await test.RunAsync();
            }

            public Task CompleteAsync()
            {
                WhereAmI();
                return Task.CompletedTask;
            }
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

        public async Task ShouldPerformOptionalAssemblyLevelBehaviorsOncePerRun()
        {
            var output = await RunSampleAsync();

            output.ShouldHaveResults(
                "FirstTestClass.Fail failed: 'Fail' failed!",
                "SecondTestClass.Pass passed");

            output.ShouldHaveLifecycle(
                "StartAsync",
                "Fail", "Pass",
                "CompleteAsync");
        }

        public async Task ShouldFailEntireRunWhenAssemblyStartThrows()
        {
            FailDuring("StartAsync");

            Func<Task> attemptInvalidRun = RunSampleAsync;

            await attemptInvalidRun.ShouldThrowAsync<FailureException>("'StartAsync' failed!");
        }

        public async Task ShouldFailEntireRunWhenAssemblyCompletionThrows()
        {
            FailDuring("CompleteAsync");

            Func<Task> attemptInvalidRun = RunSampleAsync;

            await attemptInvalidRun.ShouldThrowAsync<FailureException>("'CompleteAsync' failed!");
        }

        async Task<Output> RunSampleAsync() => await RunAsync(TestClasses, new CustomExecution());
    }
}
