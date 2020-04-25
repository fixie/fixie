namespace Fixie.Cli
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;

    static class Dotnet
    {
        public static readonly string Path = FindDotnet();

        static string FindDotnet()
        {
            var platformIsWindows = OsPlatformIsWindows();
            var fileName = platformIsWindows ? "dotnet.exe" : "dotnet";
            var separator = platformIsWindows ? ';' : ':';

            var folderPath = Environment
                .GetEnvironmentVariable("PATH")?
                .Split(separator)
                .FirstOrDefault(s => File.Exists(System.IO.Path.Combine(s, fileName)));

            if (folderPath == null)
                throw new Exception(
                    $"Could not locate {fileName} when searching the PATH environment variable. " +
                    "Verify that you have installed the .NET SDK.");

            return System.IO.Path.Combine(folderPath, fileName);
        }

        static bool OsPlatformIsWindows()
        {
            #if NET452
            return true;
            #else
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            #endif
        }
    }
}
