namespace Fixie.Console
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using Cli;

    static class Shell
    {
        public static int Run(string executable, string workingDirectory, string[] arguments)
        {
            return Run(new ProcessStartInfo
            {
                FileName = executable,
                Arguments = CommandLine.Serialize(arguments),
                WorkingDirectory = workingDirectory,
                UseShellExecute = false
            });
        }

        public static string[] RunTarget(string project, string target)
        {
            var outputPath = Path.GetTempFileName();

            try
            {
                MsBuild(project, target, outputPath);

                return File.ReadAllLines(outputPath);
            }
            finally
            {
                File.Delete(outputPath);
            }
        }

        public static string[] RunTarget(string project, string target, string configuration, string targetFramework)
        {
            var path = Path.GetTempFileName();

            try
            {
                MsBuild(project, target, configuration, targetFramework, path);

                return File.ReadAllLines(path);
            }
            finally
            {
                File.Delete(path);
            }
        }

        public static int RunTarget(string project, string target, string configuration)
            => MsBuild(project, target, configuration);

        static int MsBuild(string project, string target, string configuration = null, string targetFramework = null, string outputPath = null)
        {
            var arguments = new List<string>
            {
                "msbuild",
                project,
                "/nologo",
                "/verbosity:minimal",
                "/t:" + target
            };

            if (configuration != null)
                arguments.Add($"/p:Configuration={configuration}");

            if (targetFramework != null)
                arguments.Add($"/p:TargetFramework={targetFramework}");

            if (outputPath != null)
                arguments.Add($"/p:_Fixie_OutputFile={outputPath}");

            return Run(Dotnet.Path, workingDirectory: "", arguments.ToArray());
        }

        static int Run(ProcessStartInfo startInfo)
        {
            using (var process = Start(startInfo))
            {
                process.WaitForExit();
                return process.ExitCode;
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