namespace Fixie.VisualStudio.TestAdapter
{
    using System.Diagnostics;
    using System.IO;
    using Cli;

    public static class TestAssembly
    {
        public static Process Start(string assemblyPath)
        {
            var assemblyFullPath = Path.GetFullPath(assemblyPath);
            var assemblyDirectory = Path.GetDirectoryName(assemblyFullPath);

#if NET452
            return Start(assemblyDirectory, assemblyPath);
#else
            return Start(assemblyDirectory, Dotnet.Path, assemblyPath);
#endif
        }

        static Process Start(string workingDirectory, string executable, params string[] arguments)
        {
            return Process.Start(new ProcessStartInfo
            {
                WorkingDirectory = workingDirectory,
                FileName = executable,
                Arguments = CommandLine.Serialize(arguments)
            });
        }
    }
}