namespace Fixie.TestAdapter
{
    using System.IO;
    using System.Linq;

    static class AssemblyPath
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

        public static string FolderPath(string assemblyPath)
        {
            return new FileInfo(assemblyPath).Directory!.FullName;
        }
    }
}