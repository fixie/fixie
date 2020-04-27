namespace Fixie.TestAdapter
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using Cli;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

    public static class TestAssembly
    {
        public static bool IsTestAssembly(string assemblyPath)
        {
            var fixieAssemblies = new[]
            {
                "Fixie.dll", "Fixie.TestAdapter.dll"
            };

            if (fixieAssemblies.Contains(Path.GetFileName(assemblyPath)))
                return false;

            return File.Exists(Path.Combine(Path.GetDirectoryName(assemblyPath), "Fixie.dll"));
        }

        public static Process Start(string assemblyPath, IFrameworkHandle frameworkHandle = null)
        {
            var assemblyFullPath = Path.GetFullPath(assemblyPath);
            var assemblyDirectory = Path.GetDirectoryName(assemblyFullPath);

            return Start(frameworkHandle, assemblyDirectory, "dotnet", assemblyPath);
        }

        public static int? TryGetExitCode(this Process process)
        {
            if (process != null && process.WaitForExit(5000))
                return process.ExitCode;

            return null;
        }

        static Process Start(IFrameworkHandle frameworkHandle, string workingDirectory, string executable, params string[] arguments)
        {
            var serializedArguments = CommandLine.Serialize(arguments);

            if (Debugger.IsAttached)
            {
                // LaunchProcessWithDebuggerAttached, unlike Process.Start,
                // does not automatically propagate environment variables that
                // were created within the currently running process, so they
                // must be explicitly included here. It also does not resolve
                // bare commands (`dotnet`) to the full file path of the
                // corresponding executable, so we must do so manually.

                var environmentVariables = new Dictionary<string, string>
                {
                    ["FIXIE_NAMED_PIPE"] = Environment.GetEnvironmentVariable("FIXIE_NAMED_PIPE")
                };

                var filePath = executable == "dotnet" ? FindDotnet() : executable;

                frameworkHandle?
                    .LaunchProcessWithDebuggerAttached(
                        filePath,
                        workingDirectory,
                        serializedArguments,
                        environmentVariables);

                return null;
            }

            return Start(new ProcessStartInfo
            {
                WorkingDirectory = workingDirectory,
                FileName = executable,
                Arguments = serializedArguments,
                UseShellExecute = false
            });
        }

        static Process Start(ProcessStartInfo startInfo)
        {
            var process = new Process
            {
                StartInfo = startInfo
            };

            if (process.Start())
                return process;

            throw new Exception("Failed to start process: " + startInfo.FileName);
        }

        static string FindDotnet()
        {
            var fileName = OsPlatformIsWindows() ? "dotnet.exe" : "dotnet";

            var folderPath = Environment
                .GetEnvironmentVariable("PATH")?
                .Split(Path.PathSeparator)
                .FirstOrDefault(path => File.Exists(Path.Combine(path.Trim(), fileName)));

            if (folderPath == null)
                throw new Exception(
                    $"Could not locate {fileName} when searching the PATH environment variable. " +
                    "Verify that you have installed the .NET SDK.");

            return Path.Combine(folderPath.Trim(), fileName);
        }

        static bool OsPlatformIsWindows()
            => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    }
}