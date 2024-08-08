namespace Fixie.Tests;

class TestProject : ITestProject
{
    public void Configure(TestConfiguration configuration, TestEnvironment environment)
    {
        bool enableParallelism = true;

        if (enableParallelism)
            configuration.Conventions.Add<DefaultDiscovery, ParallelExecution>();
        else
            configuration.Conventions.Add<DefaultDiscovery, DefaultExecution>();

        if (environment.IsDevelopment())
            configuration.Reports.Add<DiffToolReport>();
        else
            configuration.Reports.Add(new GitHubReport(environment));
    }

    class ParallelExecution : IExecution
    {
        public async Task Run(TestSuite testSuite)
        {
            await Parallel.ForEachAsync(testSuite.TestClasses, async (testClass, cancellationToken) =>
            {
                foreach (var test in testClass.Tests)
                    await test.Run();
            });
        }
    }
}