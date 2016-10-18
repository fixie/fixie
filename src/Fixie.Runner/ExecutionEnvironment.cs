namespace Fixie.Runner
{
#if NET45
    using System;
    using System.Collections.Generic;
    using System.IO;
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
            assemblyResolver.RegisterAssemblyLocation(typeof(Runner).Assembly.Location);

            executionProxy = Create<ExecutionProxy>();
        }

        public int Run(IReadOnlyList<string> runnerArguments, IReadOnlyList<string> conventionArguments)
        {
            return executionProxy.Run(assemblyFullPath, runnerArguments, conventionArguments);
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
#else
    using System;
    using System.Collections.Generic;
    using System.IO;

    public class ExecutionEnvironment : IDisposable
    {
        readonly string assemblyFullPath;
        readonly string previousWorkingDirectory;
        readonly ExecutionProxy executionProxy;

        public ExecutionEnvironment(string assemblyPath)
        {
            assemblyFullPath = Path.GetFullPath(assemblyPath);

            previousWorkingDirectory = Directory.GetCurrentDirectory();
            var assemblyDirectory = Path.GetDirectoryName(assemblyFullPath);
            Directory.SetCurrentDirectory(assemblyDirectory);

            executionProxy = new ExecutionProxy();
        }

        public int Run(IReadOnlyList<string> runnerArguments, IReadOnlyList<string> conventionArguments)
        {
            return executionProxy.Run(assemblyFullPath, runnerArguments, conventionArguments);
        }

        public void Dispose()
        {
            Directory.SetCurrentDirectory(previousWorkingDirectory);
        }
    }
#endif
}