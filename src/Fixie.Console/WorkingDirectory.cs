using System;
using System.IO;
using System.Reflection;

namespace Fixie.Console
{
    public class WorkingDirectory : IDisposable
    {
        readonly string before;

        private WorkingDirectory(string path)
        {
            before = Directory.GetCurrentDirectory();

            Directory.SetCurrentDirectory(path);
        }

        public void Dispose()
        {
            Directory.SetCurrentDirectory(before);
        }

        public static WorkingDirectory LocationOf(Assembly assembly)
        {
            return new WorkingDirectory(Path.GetDirectoryName(assembly.Location));
        }
    }
}