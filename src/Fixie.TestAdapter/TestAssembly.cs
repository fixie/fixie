namespace Fixie.TestAdapter
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using Cli;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

    public static class TestAssembly
    {
        public static bool IsTestAssembly(string assemblyPath)
        {
            var fixieAssemblies = new[]
            {
                "Fixie.dll", "Fixie.TestDriven.dll", "Fixie.TestAdapter.dll"
            };

            if (fixieAssemblies.Contains(Path.GetFileName(assemblyPath)))
                return false;

            return File.Exists(Path.Combine(Path.GetDirectoryName(assemblyPath), "Fixie.dll"));
        }

        public static Process Start(string assemblyPath, IFrameworkHandle frameworkHandle = null)
        {
            var assemblyFullPath = Path.GetFullPath(assemblyPath);
            var assemblyDirectory = Path.GetDirectoryName(assemblyFullPath);

#if NET452
            return Start(frameworkHandle, assemblyDirectory, assemblyPath);
#else
            return Start(frameworkHandle, assemblyDirectory, "dotnet", assemblyPath);
#endif
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
                // must be explicitly included here.

                var environmentVariables = new Dictionary<string, string>
                {
                    ["FIXIE_NAMED_PIPE"] = Environment.GetEnvironmentVariable("FIXIE_NAMED_PIPE")
                };

                var filePath = executable == "dotnet" ? Dotnet.Path : executable;

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
    }
}