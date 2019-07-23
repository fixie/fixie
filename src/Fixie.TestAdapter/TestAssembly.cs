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
            return Start(frameworkHandle, assemblyDirectory, Dotnet.Path, assemblyPath);
#endif
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

                var processId = frameworkHandle?
                    .LaunchProcessWithDebuggerAttached(
                        executable,
                        workingDirectory,
                        serializedArguments,
                        environmentVariables);

                return TryGetProcess(processId);
            }
            else
            {
                return Start(new ProcessStartInfo
                {
                    WorkingDirectory = workingDirectory,
                    FileName = executable,
                    Arguments = serializedArguments,
                    UseShellExecute = false
                });
            }
        }

        static Process TryGetProcess(int? processId)
        {
            if (processId == null)
                return null;

            try
            {
                return Process.GetProcessById(processId.Value);
            }
            catch (Exception)
            {
                return null;
            }
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