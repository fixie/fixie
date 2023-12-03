using System.Threading.Tasks;

namespace Fixie.Tests
{
    using System;

    class TestProject : ITestProject
    {
        public void Configure(TestConfiguration configuration, TestEnvironment environment)
        {
            configuration.Conventions.Add<DefaultDiscovery, CustomExecution>();

            if (environment.IsDevelopment())
                configuration.Reports.Add<DiffToolReport>();
            else
                configuration.Reports.Add(new GitHubReport(environment));
        }

        class CustomExecution : IExecution
        {
            public async Task Run(TestSuite testSuite)
            {
                foreach (var testClass in testSuite.TestClasses)
                {
                    foreach (var test in testClass.Tests)
                    {
                        await test.Start();

                        try
                        {
                            var instance = testClass.Construct();

                            await test.Method.Call(instance);

                            await test.Pass();
                        }
                        catch (Exception failureReason)
                        {
                            await test.Fail(failureReason);
                        }
                    }
                }
            }
        }
    }
}
