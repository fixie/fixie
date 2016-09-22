namespace Fixie.Runner
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security;
    using System.Security.Permissions;
    using Execution;

    public class ExecutionEnvironment : IDisposable
    {
        readonly string assemblyFullPath;
        readonly AppDomain appDomain;
        readonly string previousWorkingDirectory;
        readonly RemoteAssemblyResolver assemblyResolver;
        readonly ExecutionProxy executionProxy;

        public ExecutionEnvironment(string assemblyPath)
        {
            assemblyFullPath = Path.GetFullPath(assemblyPath);
            appDomain = CreateAppDomain(assemblyFullPath);

            previousWorkingDirectory = Directory.GetCurrentDirectory();
            var assemblyDirectory = Path.GetDirectoryName(assemblyFullPath);
            Directory.SetCurrentDirectory(assemblyDirectory);

            assemblyResolver = CreateFrom<RemoteAssemblyResolver>();
            assemblyResolver.RegisterAssemblyLocation(typeof(ExecutionProxy).Assembly.Location);
            assemblyResolver.RegisterAssemblyLocation(typeof(SummaryListener).Assembly.Location);

            executionProxy = Create<ExecutionProxy>();
        }

        public void Subscribe<TListener>(params object[] listenerArguments)
        {
            var listenerAssemblyFullPath = typeof(TListener).Assembly.Location;
            var listenerType = typeof(TListener).FullName;

            executionProxy.Subscribe(listenerAssemblyFullPath, listenerType, listenerArguments);
        }

        public void DiscoverMethodGroups(IReadOnlyList<string> conventionArguments)
        {
            executionProxy.DiscoverMethodGroups(assemblyFullPath, conventionArguments.ToArray());
        }

        public int RunAssembly(IReadOnlyList<string> conventionArguments)
        {
            return executionProxy.RunAssembly(assemblyFullPath, conventionArguments.ToArray());
        }

        public void RunMethods(IReadOnlyList<string> methodGroups, IReadOnlyList<string> conventionArguments)
        {
            executionProxy.RunMethods(assemblyFullPath, methodGroups, conventionArguments.ToArray());
        }

        T CreateFrom<T>() where T : LongLivedMarshalByRefObject, new()
        {
            return (T)appDomain.CreateInstanceFromAndUnwrap(typeof(T).Assembly.Location, typeof(T).FullName, false, 0, null, null, null, null);
        }

        T Create<T>() where T : LongLivedMarshalByRefObject, new()
        {
            return (T)appDomain.CreateInstanceAndUnwrap(typeof(T).Assembly.FullName, typeof(T).FullName, false, 0, null, null, null, null);
        }

        public void Dispose()
        {
            executionProxy.Dispose();
            assemblyResolver.Dispose();
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