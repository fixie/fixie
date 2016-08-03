namespace Fixie.VisualStudio.TestAdapter
{
    using System;
    using System.IO;
    using System.Security;
    using System.Security.Permissions;
    using Execution;
    using Internal;

    public class ExecutionEnvironment : IDisposable
    {
        readonly string assemblyFullPath;
        readonly AppDomain appDomain;
        readonly string previousWorkingDirectory;

        public ExecutionEnvironment(string assemblyPath)
        {
            assemblyFullPath = Path.GetFullPath(assemblyPath);
            appDomain = CreateAppDomain(assemblyFullPath);

            previousWorkingDirectory = Directory.GetCurrentDirectory();
            var assemblyDirectory = Path.GetDirectoryName(assemblyFullPath);
            Directory.SetCurrentDirectory(assemblyDirectory);
        }

        public void DiscoverMethodGroups(Listener listener, Options options)
        {
            using (var executionProxy = Create<ExecutionProxy>())
                executionProxy.DiscoverMethodGroups(assemblyFullPath, options, listener);
        }

        public void RunAssembly(Listener listener, Options options)
        {
            using (var executionProxy = Create<ExecutionProxy>())
                executionProxy.RunAssembly(assemblyFullPath, options, new[] { listener });
        }

        public void RunMethods(Listener listener, Options options, MethodGroup[] methodGroups)
        {
            using (var executionProxy = Create<ExecutionProxy>())
                executionProxy.RunMethods(assemblyFullPath, options, listener, methodGroups);
        }

        T Create<T>() where T : LongLivedMarshalByRefObject
        {
            return (T)appDomain.CreateInstanceAndUnwrap(typeof(T).Assembly.FullName, typeof(T).FullName, false, 0, null, null, null, null);
        }

        public void Dispose()
        {
            AppDomain.Unload(appDomain);
            Directory.SetCurrentDirectory(previousWorkingDirectory);
        }

        static AppDomain CreateAppDomain(string assemblyFullPath)
        {
            var setup = new AppDomainSetup
            {
                ApplicationBase = Path.GetDirectoryName(assemblyFullPath),
                ApplicationName = Guid.NewGuid().ToString(),
                ConfigurationFile = GetOptionalConfigFullPath(assemblyFullPath)
            };

            return AppDomain.CreateDomain(setup.ApplicationName, null, setup, new PermissionSet(PermissionState.Unrestricted));
        }

        static string GetOptionalConfigFullPath(string assemblyFullPath)
        {
            var configFullPath = assemblyFullPath + ".config";

            return File.Exists(configFullPath) ? configFullPath : null;
        }
    }
}