namespace Fixie.Execution
{
    using System;
    using System.IO;
#if NET452
    using System.Security;
    using System.Security.Permissions;
#endif

    public class ExecutionEnvironment : IDisposable
    {
        readonly string assemblyFullPath;
#if NET452
        readonly AppDomain appDomain;
        readonly RemoteAssemblyResolver assemblyResolver;
#endif
        readonly string previousWorkingDirectory;
        readonly ExecutionProxy executionProxy;

        public ExecutionEnvironment(string assemblyPath)
        {
            assemblyFullPath = Path.GetFullPath(assemblyPath);
#if NET452
            appDomain = CreateAppDomain(assemblyFullPath);
#endif

            previousWorkingDirectory = Directory.GetCurrentDirectory();
            var assemblyDirectory = Path.GetDirectoryName(assemblyFullPath);
            Directory.SetCurrentDirectory(assemblyDirectory);

#if NET452
            assemblyResolver = CreateFrom<RemoteAssemblyResolver>();
            assemblyResolver.RegisterAssemblyLocation(typeof(ExecutionProxy).Assembly.Location);

            executionProxy = Create<ExecutionProxy>(previousWorkingDirectory);
#else
            executionProxy = new ExecutionProxy(previousWorkingDirectory);
#endif
        }

        public void Subscribe<TListener>(params object[] listenerArguments) where TListener : Listener
        {
#if NET452
            assemblyResolver.RegisterAssemblyLocation(typeof(TListener).Assembly.Location);
#endif
            executionProxy.Subscribe<TListener>(listenerArguments);
        }

        public void DiscoverMethods(string[] runnerArguments, string[] conventionArguments)
            => executionProxy.DiscoverMethods(assemblyFullPath, runnerArguments, conventionArguments);

        public int RunAssembly(string[] runnerArguments, string[] conventionArguments)
            => executionProxy.RunAssembly(assemblyFullPath, runnerArguments, conventionArguments);

        public void RunMethods(string[] runnerArguments, string[] conventionArguments, string[] methods)
            => executionProxy.RunMethods(assemblyFullPath, runnerArguments, conventionArguments, methods);

#if NET452
        T CreateFrom<T>() where T : LongLivedMarshalByRefObject, new()
        {
            return (T)appDomain.CreateInstanceFromAndUnwrap(typeof(T).Assembly.Location, typeof(T).FullName, false, 0, null, null, null, null);
        }

        T Create<T>(params object[] arguments) where T : LongLivedMarshalByRefObject
        {
            return (T)appDomain.CreateInstanceAndUnwrap(typeof(T).Assembly.FullName, typeof(T).FullName, false, 0, null, arguments, null, null);
        }
#endif

        public void Dispose()
        {
#if NET452
            executionProxy.Dispose();
            assemblyResolver.Dispose();
            AppDomain.Unload(appDomain);
#endif
            Directory.SetCurrentDirectory(previousWorkingDirectory);
        }

#if NET452
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
#endif
    }
}