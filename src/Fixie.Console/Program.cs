namespace Fixie.Console
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Cli;
    using Execution;
    using static System.Console;
    using static System.IO.Directory;
    using static Shell;

    class Program
    {
        const int FatalError = -1;

        [STAThread]
        static int Main(string[] arguments)
        {
            try
            {
                CommandLine.Partition(arguments, out var runnerArguments, out var conventionArguments);

                var options = ValidateRunnerArguments(runnerArguments);

                var testProject = TestProject();

                if (options.ShouldBuild)
                {
                    WriteLine($"Building {Path.GetFileNameWithoutExtension(testProject)}...");

                    var exitCode = msbuild(testProject, "Build", options.Configuration);

                    if (exitCode != 0)
                    {
                        Error("Build failed!");
                        return FatalError;
                    }
                }

                InjectTargets(testProject);

                var targetFrameworks = GetTargetFrameworks(options, testProject);

                var failedTests = 0;

                bool runningForMultipleFrameworks = targetFrameworks.Length > 1;
                foreach (var targetFramework in targetFrameworks)
                {
                    int exitCode = Run(options, testProject, targetFramework, conventionArguments, runningForMultipleFrameworks);

                    if (exitCode == FatalError)
                        return FatalError;

                    failedTests += exitCode;
                }

                return failedTests;
            }
            catch (CommandLineException exception)
            {
                Error(exception.Message);
                Help();

                return FatalError;
            }
            catch (Exception exception)
            {
                Error($"Fatal Error: {exception}");

                return FatalError;
            }
        }

        static Options ValidateRunnerArguments(string[] runnerArguments)
        {
            var options = CommandLine.Parse<Options>(runnerArguments, out var unusedArguments);

            foreach (var unusedArgument in unusedArguments)
                throw new CommandLineException($"The argument '{unusedArgument}' was unexpected. Custom convention arguments must appear after the argument separator '--'.");

            return options;
        }

        static string TestProject()
        {
            var testProjects = EnumerateFiles(GetCurrentDirectory(), "*.*proj").ToArray();

            if (testProjects.Length != 1)
                throw new CommandLineException($"Expected to find 1 project in the current directory, but found {testProjects.Length}.");

            return testProjects.Single();
        }

        static void InjectTargets(string testProject)
        {
            File.Copy(
                Path.Combine(PathToThisAssembly(), "dotnet-fixie.targets"),
                Path.Combine(
                    Path.GetDirectoryName(testProject),
                    "obj",
                    Path.GetFileName(testProject) + ".dotnet-fixie.targets"),
                overwrite: true);
        }

        static string[] GetTargetFrameworks(Options options, string testProject)
        {
            var targetFrameworks =
                msbuild(testProject, "_Fixie_GetTargetFrameworks")
                    .SelectMany(line => line.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries))
                    .ToArray();

            if (options.Framework == null)
                return targetFrameworks;

            if (targetFrameworks.Contains(options.Framework))
                return new[] {options.Framework};

            var availableFrameworks = string.Join(", ", targetFrameworks.Select(x => $"'{x}'"));

            throw new CommandLineException(
                $"Cannot target framework '{options.Framework}'. " +
                $"The test project targets the following framework(s): {availableFrameworks}");
        }

        static int Run(Options options, string testProject, string targetFramework, string[] conventionArguments, bool runningForMultipleFrameworks)
        {
            var assemblyMetadata = msbuild(testProject, "_Fixie_GetAssemblyMetadata", options.Configuration, targetFramework);

            var outputPath = assemblyMetadata[0];
            var assemblyName = assemblyMetadata[1];
            var targetFileName = assemblyMetadata[2];
            var targetFrameworkIdentifier = assemblyMetadata[3];

            var context =
                runningForMultipleFrameworks
                    ? $" ({targetFramework})"
                    : "";

            Heading($"Running {assemblyName}{context}");
            WriteLine();

            if (targetFrameworkIdentifier == ".NETFramework")
                return RunDotNetFramework(options, outputPath, targetFileName, conventionArguments);

            if (targetFrameworkIdentifier == ".NETCoreApp")
                return RunDotNetCore(options, outputPath, targetFileName, conventionArguments);

            throw new CommandLineException($"Framework '{targetFramework}' has unsupported TargetFrameworkIdentifier '{targetFrameworkIdentifier}'.");
        }

        static int RunDotNetFramework(Options options, string outputPath, string targetFileName, string[] conventionArguments)
        {
            var arguments = new List<string>();

            AddPassThroughArguments(arguments, options, conventionArguments);

            return run(
                executable: Path.Combine(outputPath, targetFileName),
                workingDirectory: outputPath,
                arguments: arguments.ToArray());
        }

        static int RunDotNetCore(Options options, string outputPath, string targetFileName, string[] conventionArguments)
        {
            var arguments = new List<string> { targetFileName };

            AddPassThroughArguments(arguments, options, conventionArguments);

            return dotnet(
                workingDirectory: outputPath,
                arguments: arguments.ToArray());
        }

        static string PathToThisAssembly()
            => Path.GetDirectoryName(typeof(Program).GetTypeInfo().Assembly.Location);

        static void AddPassThroughArguments(List<string> arguments, Options options, string[] conventionArguments)
        {
            foreach (var pattern in options.Patterns)
                arguments.Add(pattern);

            if (options.Report != null)
            {
                arguments.Add("--report");
                arguments.Add(options.Report);
            }

            if (options.TeamCity != null)
            {
                arguments.Add("--team-city");
                arguments.Add(options.TeamCity.Value ? "on" : "off");
            }

            arguments.Add("--");

            arguments.AddRange(conventionArguments);
        }

        static void Help()
        {
            WriteLine();
            WriteLine("Usage: dotnet fixie [patterns]... [options] [-- [convention arguments]...]");
            WriteLine();
            WriteLine();
            WriteLine("    patterns");
            WriteLine("        Zero or more test name patterns. When provided, a test");
            WriteLine("        will run only if it matches at least one of the patterns.");
            WriteLine();
            WriteLine("    --configuration name");
            WriteLine("        The configuration under which to build. When this option");
            WriteLine("        is omitted, the default configuration is `Debug`.");
            WriteLine();
            WriteLine("    --no-build");
            WriteLine("        Skip building the test project prior to running it.");
            WriteLine();
            WriteLine("    --framework name");
            WriteLine("        Only run test assemblies targeting a specific framework.");
            WriteLine();
            WriteLine("    --report path");
            WriteLine("        Write test results to the specified path, using the");
            WriteLine("        xUnit XML format.");
            WriteLine();
            WriteLine("    --team-city <on|off>");
            WriteLine("        When this option is omitted, the runner detects the need");
            WriteLine("        for TeamCity-formatted console output. Use this option");
            WriteLine("        to force TeamCity output on or off.");
            WriteLine();
            WriteLine("    --");
            WriteLine("        Signifies the end of built-in arguments and the beginning");
            WriteLine("        of custom convention arguments.");
            WriteLine();
            WriteLine("    convention arguments");
            WriteLine("        Arbitrary arguments made available to conventions.");
            WriteLine();
        }

        static void Heading(string message)
        {
            using (Foreground.Green)
                WriteLine(message);
        }

        static void Error(string message)
        {
            using (Foreground.Red)
                WriteLine(message);
        }
    }
}