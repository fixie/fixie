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

                if (FindTestProject(out string testProject))
                {
                    InjectTargets(testProject);

                    var targetFrameworks = GetTargetFrameworks(testProject);

                    return Success;
                }

                return FatalError;
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

        static string[] GetTargetFrameworks(string testProject)
            => msbuild(testProject, "_Fixie_GetTargetFrameworks")
                .SelectMany(line => line.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries))
                .ToArray();

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

        static string PathToThisAssembly()
            => Path.GetDirectoryName(typeof(Program).GetTypeInfo().Assembly.Location);

        static bool FindTestProject(out string testProject)
        {
            var testProjects = EnumerateFiles(GetCurrentDirectory(), "*.*proj").ToArray();

            if (testProjects.Length != 1)
            {
                Error($"Expected to find 1 project in the current directory, but found {testProjects.Length}.");
                testProject = null;
            }
            else
            {
                testProject = testProjects.Single();
            }

            return testProject != null;
        }

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