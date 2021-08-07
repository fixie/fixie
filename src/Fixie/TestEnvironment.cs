namespace Fixie
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Runtime.Versioning;
    using static System.Environment;

    public class TestEnvironment
    {
        internal TestEnvironment(Assembly assembly, TextWriter console, string rootPath)
            : this(assembly, console, rootPath, Array.Empty<string>()) { }

        internal TestEnvironment(Assembly assembly, TextWriter console, string rootPath,
            IReadOnlyList<string> customArguments)
        {
            Assembly = assembly;
            CustomArguments = customArguments;
            Console = console;
            RootPath = rootPath;
        }

        /// <summary>
        /// The test assembly being executed.
        /// </summary>
        public Assembly Assembly { get; }

        /// <summary>
        /// Identifies the target framework value that the test assembly was compiled against.
        /// </summary>
        public string? TargetFramework
            => Assembly
                .GetCustomAttribute<TargetFrameworkAttribute>()?
                .FrameworkName;

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
        /// AppVeyor, Azure DevOps, GitHub Actions, or TeamCity.
        /// </summary>
        public bool IsContinuousIntegration()
        {
            return 
                GetEnvironmentVariable("APPVEYOR") == "True" ||          // AppVeyor
                GetEnvironmentVariable("TF_BUILD") == "True" ||          // Azure DevOps
                GetEnvironmentVariable("GITHUB_ACTIONS") == "true" ||    // GitHub Actions
                GetEnvironmentVariable("TEAMCITY_PROJECT_NAME") != null; // TeamCity
        }
    }
}
