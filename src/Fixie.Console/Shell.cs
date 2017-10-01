namespace Fixie.Console
{
    using System.Diagnostics;
    using System.IO;
    using Cli;

    static class Shell
    {
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
            return run(Dotnet.Path, workingDirectory, arguments);
        }

        public static int dotnet(params string[] arguments)
        {
            var dotnet = new ProcessStartInfo
            {
                FileName = Dotnet.Path,
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
    }
}