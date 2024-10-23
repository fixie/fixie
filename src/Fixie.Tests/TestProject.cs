namespace Fixie.Tests;

class TestProject : ITestProject
{
    public void Configure(TestConfiguration configuration, TestEnvironment environment)
    {
        configuration.Conventions.Add(new DefaultDiscovery(), new DefaultExecution
        {
            Parallel = true
        });

        if (environment.IsDevelopment())
            configuration.Reports.Add<DiffToolReport>();
    }
}