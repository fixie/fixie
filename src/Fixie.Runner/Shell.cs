namespace Fixie.Runner
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.InteropServices;
    using Cli;

    static class Shell
    {
        static readonly string DotnetPath = FindDotnet();

        public static int run(string executable, string workingDirectory, string[] arguments)
        {
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = executable,
                Arguments = CommandLine.Serialize(arguments),
                WorkingDirectory = workingDirectory
            });
            process.WaitForExit();
            return process.ExitCode;
        }

        public static int dotnet(string workingDirectory, string[] arguments)
        {
            return run(DotnetPath, workingDirectory, arguments);
        }

        public static int dotnet(params string[] arguments)
        {
            var dotnet = new ProcessStartInfo
            {
                FileName = DotnetPath,
                Arguments = CommandLine.Serialize(arguments)
            };

            var process = Process.Start(dotnet);
            process.WaitForExit();
            return process.ExitCode;
        }

        public static string[] msbuild(string project, string target)
        {
            var path = Path.GetTempFileName();

            try
            {
                dotnet(
                    "msbuild",
                    project,
                    "/t:" + target,
                    "/nologo",
                    $"/p:_Fixie_OutputFile={path}");

                return File.ReadAllLines(path);
            }
            finally
            {
                File.Delete(path);
            }
        }

        public static string[] msbuild(string project, string target, string configuration, string targetFramework)
        {
            var path = Path.GetTempFileName();

            try
            {
                dotnet(
                    "msbuild",
                    project,
                    "/p:Configuration=" + configuration,
                    "/p:TargetFramework=" + targetFramework,
                    "/t:" + target,
                    "/nologo",
                    "/verbosity:minimal",
                    $"/p:_Fixie_OutputFile={path}");

                return File.ReadAllLines(path);
            }
            finally
            {
                File.Delete(path);
            }
        }

        public static int msbuild(string project, string target, string configuration)
            => dotnet(
                "msbuild",
                project,
                "/p:Configuration=" + configuration,
                "/t:" + target,
                "/nologo",
                "/verbosity:minimal");

        static string FindDotnet()
        {
            var fileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "dotnet.exe" : "dotnet";

            //If `dotnet` is the currently running process, return the full path to that executable.

            var mainModule = Process.GetCurrentProcess().MainModule;

            var currentProcessIsDotNet =
                !string.IsNullOrEmpty(mainModule?.FileName) &&
                Path.GetFileName(mainModule.FileName)
                    .Equals(fileName, StringComparison.OrdinalIgnoreCase);

            if (currentProcessIsDotNet)
                return mainModule.FileName;

            // Find "dotnet" by using the location of the shared framework.

            var fxDepsFile = AppContext.GetData("FX_DEPS_FILE") as string;

            if (string.IsNullOrEmpty(fxDepsFile))
                throw new CommandLineException("While attempting to locate `dotnet`, FX_DEPS_FILE could not be found in the AppContext.");

            var dotnetDirectory =
                new FileInfo(fxDepsFile) // Microsoft.NETCore.App.deps.json
                    .Directory? // (version)
                    .Parent? // Microsoft.NETCore.App
                    .Parent? // shared
                    .Parent; // DOTNET_HOME

            if (dotnetDirectory == null)
                throw new CommandLineException("While attempting to locate `dotnet`. Could not traverse directories from FX_DEPS_FILE to DOTNET_HOME.");

            var dotnetPath = Path.Combine(dotnetDirectory.FullName, fileName);

            if (!File.Exists(dotnetPath))
                throw new CommandLineException($"Failed to locate `dotnet`. The path does not exist: {dotnetPath}");

            return dotnetPath;
        }
    }
}