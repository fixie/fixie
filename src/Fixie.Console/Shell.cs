namespace Fixie.Console
{
    using System;
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
            var path = Path.GetTempFileName();

            try
            {
                MsBuild(
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

        public static string[] RunTarget(string project, string target, string configuration, string targetFramework)
        {
            var path = Path.GetTempFileName();

            try
            {
                MsBuild(
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

        public static int RunTarget(string project, string target, string configuration)
            => MsBuild(
                project,
                "/p:Configuration=" + configuration,
                "/t:" + target,
                "/nologo",
                "/verbosity:minimal");

        static int MsBuild(params string[] arguments)
        {
            return Run(Dotnet.Path, workingDirectory: "", arguments.Prepend("msbuild").ToArray());
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