namespace Fixie.Tests;

class TestProject : ITestProject
{
    public void Configure(TestConfiguration configuration, TestEnvironment environment)
    {
        configuration.Conventions.Add<DefaultDiscovery, ParallelExecution>();

        if (environment.IsDevelopment())
            configuration.Reports.Add<DiffToolReport>();
    }
}