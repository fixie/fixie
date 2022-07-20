namespace Fixie.Console
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using static System.Console;
    using static System.IO.Directory;
    using static Shell;

    class Program
    {
        const int Success = 0;
        const int Failure = 1;
        const int FatalError = -1;

        [STAThread]
        static int Main(string[] arguments)
        {
            try
            {
                CommandLine.Partition(arguments, out var runnerArguments, out var customArguments);

                var options = CommandLine.Parse<Options>(runnerArguments);

                var overallExitCode = Success;

                foreach(var testProject in TestProjects(options).ToArray())
                {
                    if (options.ShouldBuild)
                    {
                        WriteLine($"Building {Path.GetFileNameWithoutExtension(testProject)}...");

                        var exitCode = RunTarget(testProject, "Build", options.Configuration);

                        if (exitCode != 0)
                        {
                            Error("Build failed!");
                            return FatalError;
                        }
                    }

                    var targetFrameworks = GetTargetFrameworks(options, testProject);

                    bool runningForMultipleFrameworks = targetFrameworks.Length > 1;
                    foreach (var targetFramework in targetFrameworks)
                    {
                        int exitCode = RunTests(options, testProject, targetFramework, customArguments, runningForMultipleFrameworks);

                        if (exitCode != Success && exitCode != Failure)
                            Error("Unexpected exit code: " + exitCode);

                        if (exitCode != Success)
                            overallExitCode = Failure;
                    }
                }

                return overallExitCode;
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

        static IEnumerable<string> TestProjects(Options options)
        {
            if (options.ProjectPatterns.Any())
            {
                foreach (var pattern in options.ProjectPatterns)
                {
                    var found = false;

                    foreach (var project in EnumerateFiles(GetCurrentDirectory(), pattern + ".*proj", SearchOption.AllDirectories))
                    {
                        found = true;
                        yield return project;
                    }

                    if (!found)
                        throw new CommandLineException($"There are no projects matching the pattern '{pattern}'.");
                }
            }
            else
            {
                var found = false;

                foreach (var project in EnumerateFiles(GetCurrentDirectory(), "*.*proj"))
                {
                    found = true;
                    yield return project;
                }

                if (!found)
                    throw new CommandLineException("There are no projects in the current directory.");
            }
        }

        static string[] GetTargetFrameworks(Options options, string testProject)
        {
            var targetFrameworks =
                QueryTarget(testProject, "_Fixie_GetTargetFrameworks")
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

        static int RunTests(Options options, string testProject, string targetFramework, string[] customArguments, bool runningForMultipleFrameworks)
        {
            var assemblyMetadata = QueryTarget(testProject, "_Fixie_GetAssemblyMetadata", options.Configuration, targetFramework);

            var outputPath = assemblyMetadata[0];
            var assemblyName = assemblyMetadata[1];
            var targetFileName = assemblyMetadata[2];

            var context =
                runningForMultipleFrameworks
                    ? $" ({targetFramework})"
                    : "";

            Heading($"Running {assemblyName}{context}");
            WriteLine();

            var arguments = new List<string> { targetFileName };

            arguments.AddRange(customArguments);

            var workingDirectory = Path.Combine(
                new FileInfo(testProject).Directory!.FullName,
                outputPath);

            var environmentVariables = new Dictionary<string, string?>();

            if (options.Tests != null)
                environmentVariables["FIXIE:TESTS_PATTERN"] = options.Tests;

            return Run("dotnet", workingDirectory, arguments.ToArray(), environmentVariables);
        }

        static void Help()
        {
            WriteLine();
            WriteLine("Usage: dotnet fixie [project]... [options] [-- [custom arguments]...]");
            WriteLine();
            WriteLine();
            WriteLine("    project");
            WriteLine("        The name of a test project to run. When this option");
            WriteLine("        is omitted, the project in the current directory is");
            WriteLine("        assumed. Specify multiple project names or use * wildcards");
            WriteLine("        to run multiple test projects.");
            WriteLine();
            WriteLine("    -c <configuration>");
            WriteLine("    --configuration <configuration>");
            WriteLine("        The configuration under which to build. When this option");
            WriteLine("        is omitted, the default configuration is `Debug`.");
            WriteLine();
            WriteLine("    -n");
            WriteLine("    --no-build");
            WriteLine("        Skip building the test project prior to running it.");
            WriteLine();
            WriteLine("    -f <framework>");
            WriteLine("    --framework <framework>");
            WriteLine("        Only run test assemblies targeting a specific framework.");
            WriteLine();
            WriteLine("    -t <pattern>");
            WriteLine("    --tests <pattern>");
            WriteLine("        Run only the tests whose full names match the given pattern.");
            WriteLine();
            WriteLine("    --");
            WriteLine("        Signifies the end of built-in arguments and the beginning");
            WriteLine("        of custom arguments.");
            WriteLine();
            WriteLine("    custom arguments");
            WriteLine("        Arbitrary arguments made available to custom discovery/execution classes.");
            WriteLine();
        }

        static void Heading(string message)
        {
            using (Foreground.Green)
                WriteLine(message);
        }

        static void Error(string message)
        {
            WriteLine();
            using (Foreground.Red)
                WriteLine(message);
        }
    }
}