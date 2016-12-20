namespace Fixie.Execution
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Security;
    using System.Security.Permissions;
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

        public IReadOnlyList<MethodGroup> DiscoverTestMethodGroups(Options options)
        {
            using (var executionProxy = Create<ExecutionProxy>())
                return executionProxy.DiscoverTestMethodGroups(assemblyFullPath, options);
        }

        public AssemblyResult RunAssembly(Options options, Listener listener)
        {
            AssertIsLongLivedMarshalByRefObject(listener);

            using (var executionProxy = Create<ExecutionProxy>())
                return executionProxy.RunAssembly(assemblyFullPath, options, listener);
        }

        public AssemblyResult RunMethods(Options options, Listener listener, MethodGroup[] methodGroups)
        {
            AssertIsLongLivedMarshalByRefObject(listener);

            using (var executionProxy = Create<ExecutionProxy>())
                return executionProxy.RunMethods(assemblyFullPath, options, listener, methodGroups);
        }

        static void AssertIsLongLivedMarshalByRefObject(Listener listener)
        {
            if (listener is LongLivedMarshalByRefObject) return;
            var listenerType = listener.GetType();
            var message = string.Format("Type '{0}' in Assembly '{1}' must inherit from '{2}'.",
                                        listenerType.FullName,
                                        listenerType.Assembly,
                                        typeof(LongLivedMarshalByRefObject).FullName);
            throw new Exception(message);
        }

        T Create<T>(params object[] args) where T : MarshalByRefObject
        {
            return (T)appDomain.CreateInstanceAndUnwrap(typeof(T).Assembly.FullName, typeof(T).FullName, false, 0, null, args, null, null);
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