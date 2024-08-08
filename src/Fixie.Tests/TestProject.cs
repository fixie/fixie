namespace Fixie.Tests;

class TestProject : ITestProject
{
    public void Configure(TestConfiguration configuration, TestEnvironment environment)
    {
        configuration.Conventions.Add<DefaultDiscovery, ParallelExecution>();

        if (environment.IsDevelopment())
            configuration.Reports.Add<DiffToolReport>();
        else
            configuration.Reports.Add(new GitHubReport(environment));
    }

    class ParallelExecution : IExecution
    {
        public async Task Run(TestSuite testSuite)
        {
            await Parallel.ForEachAsync(testSuite.Tests, async (test, _) => await test.Run());
        }
    }
}