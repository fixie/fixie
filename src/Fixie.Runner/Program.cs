namespace Fixie.Runner
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Cli;
    using static System.Console;
    using static System.IO.Directory;
    using static Shell;

    class Program
    {
        const int FatalError = -1;
        const int Success = 0;

        [STAThread]
        static int Main(string[] arguments)
        {
            try
            {
                var options = CommandLine.Parse<Options>(arguments);

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

                return Success;
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

        static string PathToThisAssembly()
            => Path.GetDirectoryName(typeof(Program).GetTypeInfo().Assembly.Location);

        static void Help()
        {
            WriteLine();
            WriteLine("Usage: dotnet fixie [options] [convention arguments]...");
            WriteLine();
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
            WriteLine("    --x86");
            WriteLine("        Run tests in 32-bit mode. This is only applicable for");
            WriteLine("        test assemblies targeting the full .NET Framework.");
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
            WriteLine("    convention arguments");
            WriteLine("        Arbitrary arguments made available to conventions.");
            WriteLine();
        }

        static void Error(string message)
        {
            var before = ForegroundColor;
            ForegroundColor = ConsoleColor.Red;
            WriteLine(message);
            ForegroundColor = before;
        }
    }
}