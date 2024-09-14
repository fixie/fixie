using Fixie.Internal;
using Fixie.Assertions;
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
        
        var environment = new TestEnvironment(assembly, targetFrameworkVersion, console, customArguments);

        environment.TestFramework.ShouldBe(Framework.Version);
        environment.Assembly.ShouldBe(assembly);
        environment.TargetFramework.ShouldBe(targetFrameworkVersion);
        environment.Console.ShouldBe(console);
        environment.RootPath.ShouldBe(currentDirectory);
        environment.CustomArguments.ShouldMatch(["argumentA", "argumentB"]);
        environment.IsDevelopment().ShouldBe(!environment.IsContinuousIntegration());
    }

    public void ShouldInferTheTargetFrameworkFromAssemblyMetadataWhenOtherwiseUnavailable()
    {
        using var console = new StringWriter();

        string? targetFramework = null;
        
        var environment = new TestEnvironment(typeof(TestProject).Assembly, targetFramework, console, []);

        environment.TargetFramework.ShouldBe($"net{Utility.TargetFrameworkVersion}");
    }
}