namespace Fixie.Tests.Parallel
{
    public class TestProject : ITestProject
    {
        public void Configure(TestConfiguration configuration, TestEnvironment environment)
        {
            configuration.Conventions.Add<DefaultDiscovery, RunAllAtOnceExecution>();
        }
    }
}
