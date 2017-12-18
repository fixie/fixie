namespace Fixie.VisualStudio.TestAdapter
{
    using System.Diagnostics;
    using System.IO;
    using Cli;

    public static class TestAssembly
    {
        public static void Start(string assemblyPath)
        {
            var assemblyFullPath = Path.GetFullPath(assemblyPath);
            var assemblyDirectory = Path.GetDirectoryName(assemblyFullPath);

#if NET452
            Start(assemblyDirectory, assemblyPath);
#else
            Start(assemblyDirectory, Dotnet.Path, assemblyPath);
#endif
        }

        static void Start(string workingDirectory, string executable, params string[] arguments)
        {
            using (Process.Start(new ProcessStartInfo
            {
                WorkingDirectory = workingDirectory,
                FileName = executable,
                Arguments = CommandLine.Serialize(arguments),
                UseShellExecute = false
            })) { }
        }
    }
}