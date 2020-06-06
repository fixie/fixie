namespace Fixie.TestAdapter
{
    using System;
    using System.Reflection;
    using System.Runtime.Loader;

    class TestAssemblyLoadContext : AssemblyLoadContext
    {
        readonly AssemblyDependencyResolver resolver;

        public TestAssemblyLoadContext(string testAssemblyPath)
        {
            resolver = new AssemblyDependencyResolver(testAssemblyPath);
        }

        protected override Assembly? Load(AssemblyName assemblyName)
        {
            var assemblyPath = resolver.ResolveAssemblyToPath(assemblyName);

            if (assemblyPath != null)
                return LoadFromAssemblyPath(assemblyPath);

            return null;
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            var libraryPath = resolver.ResolveUnmanagedDllToPath(unmanagedDllName);

            if (libraryPath != null)
                return LoadUnmanagedDllFromPath(libraryPath);

            return IntPtr.Zero;
        }
    }
}