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

            var folderPath = new FileInfo(assemblyPath).Directory!.FullName;

            return File.Exists(Path.Combine(folderPath, "Fixie.dll"));
        }

        public static int? TryGetExitCode(this Process? process)
        {
            if (process != null && process.WaitForExit(5000))
                return process.ExitCode;

            return null;
        }

        public static Process StartDiscovery(string assemblyPath)
        {
            return Run(assemblyPath);
        }

        public static Process? StartExecution(string assemblyPath, IFrameworkHandle frameworkHandle)
        {
            if (Debugger.IsAttached)
                return Debug(assemblyPath, frameworkHandle);

            return Run(assemblyPath);
        }

        static Process Run(string assemblyPath)
        {
            var arguments = new[] { assemblyPath };

            var startInfo = new ProcessStartInfo
            {
                WorkingDirectory = WorkingDirectory(assemblyPath),
                FileName = "dotnet",
                UseShellExecute = false
            };

            foreach (var argument in arguments)
                startInfo.ArgumentList.Add(argument);

            return Start(startInfo);
        }

        static Process? Debug(string assemblyPath, IFrameworkHandle frameworkHandle)
        {
            // LaunchProcessWithDebuggerAttached sends a request back
            // to the third-party test runner process which started
            // this TestAdapter's host process. That test runner
            // process does not know about environment variables
            // created so far by this TestAdapter. That test runner
            // cannot reliably resolve the meaning of bare commands
            // like `dotnet` to the full file path of the corresponding
            // executable. To ensure the test runner process can
            // successfully honor the request, we must explicitly
            // pass along new environment variables and resolve the
            // full path for the `dotnet` executable.

            var arguments = new[] { assemblyPath };

            var environmentVariables = new Dictionary<string, string?>
            {
                ["FIXIE_NAMED_PIPE"] = Environment.GetEnvironmentVariable("FIXIE_NAMED_PIPE")
            };

            var filePath = FindDotnet();

            frameworkHandle
                .LaunchProcessWithDebuggerAttached(
                    filePath,
                    WorkingDirectory(assemblyPath),
                    Serialize(arguments),
                    environmentVariables);

            return null;
        }

        static string WorkingDirectory(string assemblyPath)
        {
            return Path.GetDirectoryName(Path.GetFullPath(assemblyPath))!;
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