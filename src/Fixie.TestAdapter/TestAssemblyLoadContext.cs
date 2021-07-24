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
            if (assemblyName.Name == "Fixie")
            {
                // The Fixie assembly itself must be loaded only
                // into the AssemblyLoadContext.Default context,
                // so that the Test Adapter can share types with
                // those loaded by the test assembly such as Report,
                // Handler<T>, Execution, and IDiscovery. Otherwise,
                // both contexts would have distinct occurrences of
                // those types, and they would be mistaken as unrelated.
            
                return null;
            }

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