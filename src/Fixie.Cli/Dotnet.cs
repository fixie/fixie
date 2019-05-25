namespace Fixie.Cli
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.InteropServices;

    static class Dotnet
    {
        public static readonly string Path = FindDotnet();

        static string FindDotnet()
        {
            var fileName = OsPlatformIsWindows() ? "dotnet.exe" : "dotnet";

            //If `dotnet` is the currently running process, return the full path to that executable.

            using (var currentProcess = Process.GetCurrentProcess())
            {
                var mainModule = currentProcess.MainModule;

                var currentProcessIsDotNet =
                    !string.IsNullOrEmpty(mainModule?.FileName) &&
                    System.IO.Path.GetFileName(mainModule.FileName)
                        .Equals(fileName, StringComparison.OrdinalIgnoreCase);

                if (currentProcessIsDotNet)
                    return mainModule.FileName;

                return "dotnet";
            }
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
