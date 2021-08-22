namespace Fixie.TestAdapter
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text.RegularExpressions;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

    static class TestAssembly
    {
        public static bool IsTestAssembly(string assemblyPath)
        {
            var fixieAssemblies = new[]
            {
                "Fixie.dll", "Fixie.TestAdapter.dll"
            };

            if (fixieAssemblies.Contains(Path.GetFileName(assemblyPath)))
                return false;

            return File.Exists(Path.Combine(FolderPath(assemblyPath), "Fixie.dll"));
        }

        static string FolderPath(string assemblyPath)
        {
            return new FileInfo(assemblyPath).Directory!.FullName;
        }

        public static Process? Start(string assemblyPath, IFrameworkHandle? frameworkHandle = null)
        {
            var assemblyFullPath = Path.GetFullPath(assemblyPath);
            var assemblyDirectory = Path.GetDirectoryName(assemblyFullPath)!;

            return Start(frameworkHandle, assemblyDirectory, "dotnet", assemblyPath);
        }

        public static int? TryGetExitCode(this Process? process)
        {
            if (process != null && process.WaitForExit(5000))
                return process.ExitCode;

            return null;
        }

        static Process? Start(IFrameworkHandle? frameworkHandle, string workingDirectory, string executable, params string[] arguments)
        {
            var serializedArguments = Serialize(arguments);

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
                    ["FIXIE_NAMED_PIPE"] = Environment.GetEnvironmentVariable("FIXIE_NAMED_PIPE")!
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
            var fileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "dotnet.exe" : "dotnet";

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

        /// <summary>
        /// Serialize the given string[] to a single string, so that when used as a ProcessStartInfo.Arguments
        /// value, the process's Main method will receive the original string[].
        /// 
        /// See https://blogs.msdn.microsoft.com/twistylittlepassagesallalike/2011/04/23/everyone-quotes-command-line-arguments-the-wrong-way/
        /// See https://stackoverflow.com/a/6040946 for the regex approach used here.
        /// </summary>
        public static string Serialize(string[] arguments)
            => string.Join(" ", arguments.Select(Quote));

        static string Quote(string argument)
        {
            //For each substring of zero or more \ followed by "
            //replace it with twice as many \ followed by \"
            var s = Regex.Replace(argument, @"(\\*)" + '"', @"$1$1\" + '"');

            //When an argument ends in \ double the number of \ at the end.
            s = Regex.Replace(s, @"(\\+)$", @"$1$1");

            //Now that the content has been escaped, surround the value in quotes.
            return '"' + s + '"';
        }
    }
}