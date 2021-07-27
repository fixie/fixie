namespace Fixie.Tests
{
    using static System.Environment;

    class TestProject : ITestProject
    {
        public void Configure(Configuration configuration, TestContext context)
        {
            if (GetEnvironmentVariable("GITHUB_ACTIONS") == null)
                configuration.Reports.Add<DiffToolReport>();
        }
    }
}
