using System.Reflection;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using Fixie.Internal;
using static System.Environment;

namespace Fixie;

public class TestEnvironment
{
    internal TestEnvironment(Assembly assembly, string? targetFramework, TextWriter console, IReadOnlyList<string> customArguments)
    {
        TargetFramework = targetFramework ?? InferTargetFramework(assembly);
        Assembly = assembly;
        CustomArguments = customArguments;
        Console = console;
        RootPath = Path.GetDirectoryName(assembly.Location) ?? "";
    }

    string? InferTargetFramework(Assembly assembly)
    {
        var name = assembly.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName;
        
        if (name == null)
            return null;
        
        return Regex.Replace(name,
            @"^\.NETCoreApp,Version=v([\d\.]+)$",
            "net$1");
    }

    /// <summary>
    /// The name and version of the test framework.
    /// </summary>
    public string TestFramework => Framework.Version;

    /// <summary>
    /// The test assembly being executed.
    /// </summary>
    public Assembly Assembly { get; }

    /// <summary>
    /// Identifies the target framework value that the test assembly was compiled against.
    /// </summary>
    public string? TargetFramework { get; }

    /// <summary>
    /// Optional custom command line arguments provided to the test runner.
    /// </summary>
    public IReadOnlyList<string> CustomArguments { get; }

    /// <summary>
    /// The standard output stream. Use this to reliably write to the test
    /// runner's original standard output stream, even if tests or the
    /// system under test have redirected System.Console.Out.
    /// </summary>
    public TextWriter Console { get; }
        
    /// <summary>
    /// The absolute path to the directory containing the test assembly.
    /// </summary>
    public string RootPath { get; }

    /// <summary>
    /// Returns true when running in a local development environment. See its
    /// inverse, IsContinuousIntegration().
    /// </summary>
    /// <returns></returns>
    public bool IsDevelopment() => !IsContinuousIntegration();

    /// <summary>
    /// Returns true when running in a recognized Continuous Integration environment:
    /// Azure DevOps, GitHub Actions, or TeamCity.
    /// </summary>
    public bool IsContinuousIntegration()
    {
        return 
            GetEnvironmentVariable("TF_BUILD") == "True" ||          // Azure DevOps
            GetEnvironmentVariable("GITHUB_ACTIONS") == "true" ||    // GitHub Actions
            GetEnvironmentVariable("TEAMCITY_PROJECT_NAME") != null; // TeamCity
    }
}