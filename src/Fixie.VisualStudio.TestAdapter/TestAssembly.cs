namespace Fixie.VisualStudio.TestAdapter
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using Cli;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

    public static class TestAssembly
    {
        public static void Start(string assemblyPath, IFrameworkHandle frameworkHandle = null)
        {
            var assemblyFullPath = Path.GetFullPath(assemblyPath);
            var assemblyDirectory = Path.GetDirectoryName(assemblyFullPath);

#if NET452
            Start(frameworkHandle, assemblyDirectory, assemblyPath);
#else
            Start(frameworkHandle, assemblyDirectory, Dotnet.Path, assemblyPath);
#endif
        }

        static void Start(IFrameworkHandle frameworkHandle, string workingDirectory, string executable, params string[] arguments)
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

                frameworkHandle?
                    .LaunchProcessWithDebuggerAttached(
                        executable,
                        workingDirectory,
                        serializedArguments,
                        environmentVariables);
            }
            else
            {
                using (Process.Start(new ProcessStartInfo
                {
                    WorkingDirectory = workingDirectory,
                    FileName = executable,
                    Arguments = serializedArguments,
                    UseShellExecute = false
                })) { }
            }
        }
    }
}