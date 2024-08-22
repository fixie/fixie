using Fixie.Internal;

namespace Fixie.Tests;

public class TestEnvironmentTests
{
    public void ShouldDescribeTheTestingEnvironment()
    {
        var assembly = typeof(TestProject).Assembly;
        var targetFrameworkVersion = $"net{Utility.TargetFrameworkVersion}";
        using var console = new StringWriter();
        var currentDirectory = Directory.GetCurrentDirectory();
        string[] customArguments = ["argumentA", "argumentB"];
        
        var environment = new TestEnvironment(assembly, targetFrameworkVersion, console, currentDirectory, customArguments);

        environment.TestFramework.ShouldBe(Framework.Version);
        environment.Assembly.ShouldBe(assembly);
        environment.TargetFramework.ShouldBe(targetFrameworkVersion);
        environment.Console.ShouldBe(console);
        environment.RootPath.ShouldBe(currentDirectory);
        environment.CustomArguments.ShouldBe(["argumentA", "argumentB"]);
        environment.IsDevelopment().ShouldBe(!environment.IsContinuousIntegration());
    }

    public void ShouldInferTheTargetFrameworkFromAssemblyMetadataWhenOtherwiseUnavailable()
    {
        using var console = new StringWriter();

        string? targetFramework = null;
        
        var environment = new TestEnvironment(typeof(TestProject).Assembly, targetFramework, console, Directory.GetCurrentDirectory(), []);

        environment.TargetFramework.ShouldBe($"net{Utility.TargetFrameworkVersion}");
    }
}