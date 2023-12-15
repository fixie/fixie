namespace Fixie.Tests;

class TestProject : ITestProject
{
    public void Configure(TestConfiguration configuration, TestEnvironment environment)
    {
        if (environment.IsDevelopment())
            configuration.Reports.Add<DiffToolReport>();
        else
            configuration.Reports.Add(new GitHubReport(environment));

        configuration.Conventions.Add<DefaultDiscovery, CustomExecution>();
    }
}

class CustomExecution : IExecution
{
    public async Task Run(TestSuite testSuite)
    {
        System.Console.WriteLine("Entering CustomExecution STA Setup");
        if (OperatingSystem.IsWindows())
        {
            System.Console.WriteLine("\tCurrent Thread Apartment State: " + Thread.CurrentThread.GetApartmentState());
            System.Console.WriteLine("\tAttempting to set Apartment State to STA...");
            Thread.CurrentThread.SetApartmentState(ApartmentState.Unknown);
            Thread.CurrentThread.SetApartmentState(ApartmentState.STA);
            System.Console.WriteLine("\tCurrent Thread Apartment State: " + Thread.CurrentThread.GetApartmentState());
        }
        System.Console.WriteLine("Exiting CustomExecution STA Setup");

        foreach (var test in testSuite.Tests)
            await test.Run();
    }
}